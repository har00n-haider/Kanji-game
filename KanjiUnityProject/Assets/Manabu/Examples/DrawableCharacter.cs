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
    public class Stroke
    {
        public class ReferenceStroke
        {
            public List<Vector2> keyPoints = new List<Vector2>(); // key points in the stroke used for evaluation
            public List<Vector2> points = new List<Vector2>();    // points used for visualising the line on screen
            public float length { get; private set; }

            public ReferenceStroke(Vector3 scale, int id, List<Vector2> points, int noOfKeyPoints)
            {
                this.points.AddRange(points);
                keyPoints = SVGUtils.GenRefPntsForPnts(this.points, noOfKeyPoints);
                length = SVGUtils.GetLengthForPnts(this.points);
                // Reference points are 0 - 1, need to scale up to fit the collider
                for (int i = 0; i < this.points.Count; i++)
                {
                    this.points[i].Scale(scale);
                }
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

            public InputStroke(int id, int noOfKeyPoints)
            {
                this.noOfKeyPoints = noOfKeyPoints;
            }

            public void AddPoint(Vector2 point)
            {
                points.Add(point);
            }

            public void Complete()
            {
                keyPoints = SVGUtils.GenRefPntsForPnts(points, noOfKeyPoints);
                length = SVGUtils.GetLengthForPnts(points);
                completed = true;
            }
        }

        public InputStroke inpStroke = null;
        public ReferenceStroke refStroke = null;
        public bool completed { get { return inpStroke.completed; } }
        public DrawConfiguration config = null;

        // results
        public bool pass = true;
        public List<float?> keyPointDeltas = new List<float?>(); 
        public int tightPointIdx = -1;

        public Stroke(int strokeId, DrawableCharacter drawChar, DrawConfiguration config)
        {
            this.config = config;
            // generate ref stroke
            refStroke = new ReferenceStroke(
                drawChar.boxCollider.size, 
                strokeId, 
                drawChar.Character.drawData.strokes[strokeId].points,
                config.noRefPointsInStroke);
            // generate input stroke
            inpStroke = new InputStroke(strokeId, config.noRefPointsInStroke);
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

    [Serializable]
    public class DrawConfiguration
    {
        [Header("Evaluation")]
        // configuration for strokes
        public int noRefPointsInStroke;
        public float compThreshTight;
        public float compThreshLoose;
        public float lengthThreshold;
    }

    /// <summary>
    /// Example of how to use the character data from from Manabu to create a 
    /// drawable character on the screen
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class DrawableCharacter : MonoBehaviour
    {
        // scale
        [SerializeField]
        private Vector3 boxColliderSize; // width/height size
        [SerializeField]
        public float kanjiZBoxRelativepos = 0.5f;
        /// <summary>
        /// used to scale the normalised kanji points to the the dims of the box
        /// </summary>
        [HideInInspector]
        public BoxCollider boxCollider = null;

        // state
        private List<Stroke> strokes = new List<Stroke>();
        private Stroke curStroke { get { return curStrokeIdx < strokes.Count ? strokes[curStrokeIdx] : null; } }
        private int curStrokeIdx = 0;
        public bool completed { get; private set; } = false;

        // character data
        public DrawConfiguration config { get { return _config; } private set { _config = value; } }
        public Character Character { get; private set; } = null;
        [SerializeField]
        private DrawConfiguration _config;
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
            boxCollider = GetComponent<BoxCollider>();
            this.Character = charData;

            // generate the pairs of strokes that will take the input points, one stroke at a time
            for (int strokeId = 0; strokeId < charData.drawData.strokes.Count; strokeId++)
            {
                // assuming we get these in order
                strokes.Add(new Stroke(strokeId, this, config));
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
            if (boxCollider == null) return new Plane();
            // create the plane on which the character will be drawn
            Vector3 planePoint = transform.TransformPoint(boxCollider.center);
            Vector3 planeDir = -gameObject.transform.forward;
            return new Plane(planeDir.normalized, planePoint);
        }

        public void Reset()
        {
            // state
            strokes.Clear();
            curStrokeIdx = 0;
            completed = false;

            // data
            Character = null;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
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

        private void DrawStrokeEvaluation(Stroke sp)
        {
            if (sp.completed)
            {
                for (int i = 0; i < config.noRefPointsInStroke; i++)
                {
                    float radius = boxCollider.size.magnitude / 110.0f;
                    Gizmos.color = Color.gray;
                    var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.keyPoints[i].x, sp.refStroke.keyPoints[i].y, kanjiZBoxRelativepos));
                    Gizmos.DrawSphere(refPnt, radius);
                    Gizmos.color = new Color(0, 0, 0, 0.1f);
                    Gizmos.DrawSphere(refPnt, config.compThreshLoose);
                    Gizmos.DrawSphere(refPnt, config.compThreshTight);
                    Gizmos.color = sp.pass ? Color.green : Color.red;
                    // tight dist color
                    Gizmos.color = sp.tightPointIdx == i ? new Color(1, 0, 1) : Gizmos.color; // purple
                    var inpPnt = transform.TransformPoint(new Vector3(sp.inpStroke.keyPoints[i].x, sp.inpStroke.keyPoints[i].y, kanjiZBoxRelativepos));
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
                    Vector3 start = transform.TransformPoint(new Vector3(l[i - 1].x, l[i - 1].y, kanjiZBoxRelativepos));
                    Vector3 end = transform.TransformPoint(new Vector3(l[i].x, l[i].y, kanjiZBoxRelativepos));
                    Debug.DrawLine(start, end, c);
                }
            };

            foreach (var s in strokes)
            {
                drawStroke(s.refStroke.points, Color.green);
            }

            if (curStroke != null) drawStroke(curStroke.inpStroke.points, Color.blue);
        }

#endif

    }
}