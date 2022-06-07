using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Manabu.Core;
using System;

namespace Manabu.Examples
{
    /// <summary>
    /// Container for a given stroke that makes comparison between user input and reference data,
    /// based on a number of key points in 
    /// </summary>
    public class DrawableStroke
    {
        public class ReferenceStroke
        {
            public List<Vector2> keyPoints = new List<Vector2>(); // key points in the stroke used for evaluation
            public List<Vector2> points = new List<Vector2>();    // points used for visualising the line on screen
            public float length { get; private set; }

            public ReferenceStroke(Vector2 scale, List<Vector2> points, int noOfKeyPoints)
            {
                this.points.AddRange(points);
                // Reference points are 0 - 1, need to scale up to fit the size
                for (int i = 0; i < this.points.Count; i++)
                {
                    this.points[i] = Vector2.Scale(this.points[i],scale);
                }
                //keyPoints = SVGUtils.GenRefPntsForPnts(this.points, noOfKeyPoints);
                length = SVGUtils.GetLengthForPnts(this.points);
            }
        }

        public class InputStroke
        {
            // stats/data for the stroke
            public List<Vector2> keyPoints = new(); // key points in the stroke used for evaluation
            public List<Vector2> points = new();    // points used for visualising the line on screen
            public float length { get; private set; }
            public bool completed;
            public bool active;
            private int noOfKeyPoints;

            public InputStroke(int noOfKeyPoints)
            {
                this.noOfKeyPoints = noOfKeyPoints;
            }

            public void AddPoint(Vector2 point)
            {
                points.Add(point);
            }

            public void Complete()
            {
                //keyPoints = SVGUtils.GenRefPntsForPnts(points, noOfKeyPoints);
                length = SVGUtils.GetLengthForPnts(points);
                completed = true;
            }
        }

        [Serializable]
        public class DrawableStrokeConfig
        {
            [Header("Stroke evaluation")]
            // configuration for strokes
            public int noRefPointsInStroke = 5;
            public float compThreshTight = 0.03f;
            public float compThreshLoose = 0.07f;
            public float lengthThreshold = 2;
        }

        public InputStroke inpStroke = null;
        public ReferenceStroke refStroke = null;
        public bool completed { get { return inpStroke.completed; } }
        public DrawableStrokeConfig config = new DrawableStrokeConfig();

        // results
        public bool pass = true;
        public List<float?> keyPointDeltas = new List<float?>(); 
        public int tightPointIdx = -1;

        public DrawableStroke(Vector2 size, List<Vector2> points)
        {
            // generate ref stroke
            refStroke = new ReferenceStroke(size, points, config.noRefPointsInStroke);
            // generate input stroke
            inpStroke = new InputStroke(config.noRefPointsInStroke);
            inpStroke.active = false;
        }

        public void EvaluateStroke()
        {
            // all points need to be under the loose threshold
            for (int i = 0; i < inpStroke.keyPoints.Count; i++)
            {
                float distance = Mathf.Abs((inpStroke.keyPoints[i] - refStroke.keyPoints[i]).magnitude);
                keyPointDeltas.Add(distance);
                pass &= distance < config.compThreshLoose;
            }
            // at least one point needs to be under the tight thresh
            float? tightDist = keyPointDeltas.FirstOrDefault(d => d < config.compThreshTight);
            if (tightDist != null) tightPointIdx = keyPointDeltas.IndexOf(tightDist);
            pass &= tightPointIdx != -1;
            // total length needs to be within limits
            pass &= Mathf.Abs(inpStroke.length - refStroke.length) < config.lengthThreshold;
        }
    }

    /// <summary>
    /// Example of how to use the character data from from Manabu to create a 
    /// drawable character on the screen
    /// </summary>
    public class DrawableCharacter : MonoBehaviour
    {
        // scale
        /// <summary>
        /// Width & height of the character in world units. Used to scale the 0 - 1 range of the Manabu character
        /// </summary>
        public Vector3 CharacterSize { get { return characterSize; }  }
        [SerializeField]
        private Vector3 characterSize;
        public Vector3 CharacterCenter { get { return new Vector3(0.5f * characterSize.x, 0.5f * characterSize.y, 0.5f * characterSize.z); } }


        // state
        private List<DrawableStroke> strokes = new List<DrawableStroke>();
        private DrawableStroke curStroke { get { return curStrokeIdx < strokes.Count ? strokes[curStrokeIdx] : null; } }
        private int curStrokeIdx = 0;
        public bool completed { get; private set; } = false;

        // character data
        public Character Character { get; private set; } = null;
        [SerializeField]
        private char character;
        [SerializeField]
        private TextAsset databaseFile;

        // events
        public event Action OnCompleted;

        private void Start()
        {
            // would do this somewhere else (management object etc.) , but done here for the example
            Database database = new Database();
            database.Load(databaseFile);
            Character = database.GetCharacter(character);
            Init(Character);
            Debug.Log("Enable gizmos on game view to see the character");
        }

        private void Init(Character charData)
        {
            this.Character = charData;

            // generate the pairs of strokes that will take the input points, one stroke at a time
            for (int strokeId = 0; strokeId < charData.drawData.strokes.Count; strokeId++)
            {
                // assuming we get these in order
                strokes.Add(new DrawableStroke(characterSize, charData.drawData.strokes[strokeId].points));
            };

            // start the looking for the first stroke
            curStrokeIdx = 0;
            strokes[0].inpStroke.active = true;
        }

        private void Update()
        {
            if (completed || strokes.Count == 0) return;
            UpdateInput();
            // process current stroke, move to the next if done
            if (curStroke.completed)
            {
                curStroke.EvaluateStroke();
                curStroke.inpStroke.active = false;
                if (curStroke == strokes.Last())
                {
                    completed = true;
                    return;
                }
                else
                {
                    curStrokeIdx++;
                    curStroke.inpStroke.active = true;
                }
            };
            // deal with a completed character
            if (completed) OnCompleted?.Invoke();
        }

        private void UpdateInput()
        {
            // populate line
            bool buttonPressed = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
            if (buttonPressed)
            {
                // convert mouse position to a point on the kanji plane 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                bool hit = GetPlane().Raycast(ray, out float enter);
                if (hit)
                {
                    // normalize the input points for correct comparison with ref stroke
                    Vector3 worldPoint = ray.direction * enter + ray.origin;
                    Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                    curStroke.inpStroke.AddPoint(localPoint);
                }

            }
            // clear line
            bool buttonReleased = Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space);
            if (buttonReleased)
            {
                curStroke.inpStroke.Complete();
            }
        }

        // Get the plane on which the the character lies
        private Plane GetPlane()
        {
            // create the plane on which the character will be drawn
            Vector3 planePoint = transform.TransformPoint(CharacterCenter);
            Vector3 planeDir = -gameObject.transform.forward;
            return new Plane(planeDir.normalized, planePoint);
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            // Box enclosing the character
            DrawBox(transform.TransformPoint(CharacterCenter), transform.rotation, characterSize, new Color(0, 1 ,0 , 0.5f));

            // draw debug strokes
            DrawStrokePair();

            if (strokes.Count > 0)
            {
                for (int i = 0; i <= curStrokeIdx; i++)
                {
                    DrawStrokeEvaluation(strokes[i]);
                }
            }
        }

        private void DrawStrokeEvaluation(DrawableStroke sp)
        {
            if (sp.completed)
            {
                for (int i = 0; i < sp.config.noRefPointsInStroke; i++)
                {
                    float radius = characterSize.magnitude / 110.0f;
                    Gizmos.color = Color.gray;
                    var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.keyPoints[i].x, sp.refStroke.keyPoints[i].y, CharacterCenter.z));
                    Gizmos.DrawSphere(refPnt, radius);
                    Gizmos.color = new Color(0, 0, 0, 0.1f);
                    Gizmos.DrawSphere(refPnt, sp.config.compThreshLoose);
                    Gizmos.DrawSphere(refPnt, sp.config.compThreshTight);
                    Gizmos.color = sp.pass ? Color.green : Color.red;
                    // tight dist color
                    Gizmos.color = sp.tightPointIdx == i ? new Color(1, 0, 1) : Gizmos.color; // purple
                    var inpPnt = transform.TransformPoint(new Vector3(sp.inpStroke.keyPoints[i].x, sp.inpStroke.keyPoints[i].y, CharacterCenter.z));
                    Gizmos.DrawSphere(inpPnt, radius);
                    // connect the two
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(refPnt, inpPnt);
                }
            }
        }

        private void DrawStrokePair()
        {
            Action<List<Vector2>, Color> drawStroke = (l, c) =>
            {
                for (int i = 1; i < l.Count; i++)
                {
                    Vector3 start = transform.TransformPoint(new Vector3(l[i - 1].x, l[i - 1].y, CharacterCenter.z));
                    Vector3 end = transform.TransformPoint(new Vector3(l[i].x, l[i].y, CharacterCenter.z));
                    Debug.DrawLine(start, end, c);
                }
            };

            foreach (var s in strokes)
            {
                drawStroke(s.refStroke.points, Color.green);
            }

            if (curStroke != null) drawStroke(curStroke.inpStroke.points, Color.blue);
        }

        public void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c)
        {
            // create matrix
            Matrix4x4 m = new Matrix4x4();
            m.SetTRS(pos, rot, scale);

            var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
            var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
            var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
            var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

            var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
            var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
            var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
            var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

            Debug.DrawLine(point1, point2, c);
            Debug.DrawLine(point2, point3, c);
            Debug.DrawLine(point3, point4, c);
            Debug.DrawLine(point4, point1, c);

            Debug.DrawLine(point5, point6, c);
            Debug.DrawLine(point6, point7, c);
            Debug.DrawLine(point7, point8, c);
            Debug.DrawLine(point8, point5, c);

            Debug.DrawLine(point1, point5, c);
            Debug.DrawLine(point2, point6, c);
            Debug.DrawLine(point3, point7, c);
            Debug.DrawLine(point4, point8, c);
        }

#endif

    }
}