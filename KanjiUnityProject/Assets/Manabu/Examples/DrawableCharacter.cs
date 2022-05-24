using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Manabu.Core;
using System;

namespace Manabu.Examples
{


    public class StrokeResult
    {
        public bool pass = false;

        // same order and size as the refpoints
        public List<float?> refPointDistances = new List<float?>();

        public int tightPointIdx = -1;
    }

    public class StrokePair
    {
        public DrawableStroke inpStroke = null;
        public DrawableStroke refStroke = null;
        public StrokeResult strokeResult = null;
        public bool isValid { get { return inpStroke.isValid && refStroke.isValid; } }
    }

    [Serializable]
    public class DrawConfiguration
    {
        // configuration for strokes
        public int noRefPointsInStroke { get; private set; } = 5;

        public float compThreshTight = 0.3f;
        public float compThreshLoose = 0.7f;
        public float lengthBuffer = 1f;
        public Color wrongColor;
        public Color correctColor;
        public Color hintColor;
        public Color completedColor;
        public Color drawnColor;
        public float thickness;
    }

    /// <summary>
    /// Example of how to use the character data from from Manabu to create a 
    /// drawable character on the screen
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class DrawableCharacter : MonoBehaviour
    {
        // box resize stuff
        // used to scale the normalised kanji points to the the dims of the box
        [HideInInspector]
        public BoxCollider boxCollider = null;
        // width/height size
        [SerializeField]
        private Vector3 boxColliderSize;
        [SerializeField]
        public float kanjiZBoxRelativepos = 0.5f;

        // grid
        private CharacterGrid charGrid = null;
        [SerializeField]
        private float gridThickness;

        [SerializeField]
        private char character;

        [SerializeField]
        private TextAsset databaseFile;

        // current state of the kanji
        private Dictionary<int, StrokePair> strokes = new Dictionary<int, StrokePair>();
        private StrokePair curStroke { get { return strokes[curStrokeIdx]; } }
        private int curStrokeIdx = 0;
        public bool completed { get; private set; } = false;
        public bool pass { get; private set; } = false;
        public float score { get; private set; } = 0;

        // kanji data
        [SerializeField]
        private DrawConfiguration _config;
        public DrawConfiguration config { get { return _config; } private set { _config = value; } }
        public Character charData { get; private set; } = null;

        // refs
        public DrawableStroke strokePrefab;

        private void Start()
        {
            Database database = new Database();
            database.Load(databaseFile);
            charData = database.GetCharacter(character);
            Init(charData);
        }

        /// <summary>
        /// Handles the loading of a kanji from the parser
        /// </summary>
        /// <param name="characterData"></param>
        /// <param name="scale"></param>
        private void Init(Character characterData)
        {

            // set up before initialising the base class (need the collider set up)
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider>();
                ResizeCollider();
            }
            // setup the grid
            if (charGrid == null)
            {
                charGrid = GetComponentInChildren<CharacterGrid>();
                charGrid.Init(charData.drawData, boxCollider, gridThickness);
            }

            // pull a kanji
            bool refKanjiHidden = characterData.progress.flawlessClears >= 3;
            for (int sIdx = 0; sIdx < characterData.drawData.strokes.Count; sIdx++)
            {
                // assuming we get these in order
                strokes.Add(
                    sIdx,
                    new StrokePair()
                    {
                        refStroke = GenerateRefStroke(characterData.drawData.strokes[sIdx], refKanjiHidden),
                        inpStroke = GenerateInpStroke()
                    });
            }
            curStrokeIdx = 0;
            this.charData = characterData;
            // start the looking for the first stroke
            strokes[0].inpStroke.gameObject.SetActive(true);
        }

        private void ResizeCollider()
        {
            Vector3 halfSize = boxColliderSize / 2;
            boxCollider.size = boxColliderSize;
            boxCollider.center = halfSize;
        }

        /// <summary>
        /// The input has to be provided in normalised coordinates (0-1) in this coordinate system:
        ///  y/\
        ///   |
        ///   |
        ///    -----> x
        /// relative to a rect in which the kanji exists
        /// </summary>
        private void UpdateInput()
        {
            // populate line
            bool buttonPressed = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
            if (buttonPressed)
            {
                // convert mouse position to a point on the kanji plane 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (boxCollider.Raycast(ray, out RaycastHit hitInfo, 10000))
                {

                    bool hit = GetPlane().Raycast(ray, out float enter);
                    if (hit)
                    {
                        Debug.Log("ray hit plane");
                        // normalize the input points for correct comparison with ref stroke
                        Vector3 worldPoint = ray.direction * enter + ray.origin;
                        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                        Vector3 normLocPoint = GeometryUtils.NormalizePointToBoxPosOnly(boxCollider.size, localPoint);
                        curStroke.inpStroke.AddPoint(normLocPoint);
                    }
                };


            }

            // clear line
            bool buttonReleased = Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space);
            if (buttonReleased)
            {
                curStroke.inpStroke.Complete();
            }
        }

        // Get the plane on which the the 3d kanji lies
        private Plane GetPlane()
        {
            if (boxCollider == null) return new Plane();
            // create the plane on which the kanji will be drawn
            Vector3 planePoint = transform.TransformPoint(boxCollider.center);
            Vector3 planeDir = -gameObject.transform.forward;
            return new Plane(planeDir.normalized, planePoint);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // plane
            Plane kanjiPlane = GetPlane();
            DrawPlane(kanjiPlane, kanjiPlane.ClosestPointOnPlane(transform.position), new Color(0, 0, 1, 0.1f));

            // draw debug strokes
            if (strokes.Count > 0)
            {
                for (int i = 0; i <= curStrokeIdx; i++)
                {
                    if (strokes[i].strokeResult != null) DrawStrokePair(strokes[i]);
                }
            }
        }

        private void DrawStrokePair(StrokePair sp)
        {
            if (sp.isValid)
            {
                for (int i = 0; i < config.noRefPointsInStroke; i++)
                {
                    Gizmos.color = Color.gray;
                    var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.refPoints[i].x, sp.refStroke.refPoints[i].y));
                    Gizmos.DrawSphere(refPnt, 0.1f);
                    Gizmos.color = new Color(0, 0, 0, 0.1f);
                    Gizmos.DrawSphere(refPnt, config.compThreshLoose);
                    Gizmos.DrawSphere(refPnt, config.compThreshTight);
                    Gizmos.color = sp.strokeResult.pass ? Color.green : Color.red;
                    // tight dist color
                    Gizmos.color = sp.strokeResult.tightPointIdx == i ? new Color(1, 0, 1) : Gizmos.color; // purple
                    var inpPnt = transform.TransformPoint(new Vector3(sp.inpStroke.refPoints[i].x, sp.inpStroke.refPoints[i].y));
                    Gizmos.DrawSphere(inpPnt, 0.1f);
                    // connect the two
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(refPnt, inpPnt);
                }
            }
        }

        private void DrawPlane(Plane p, Vector3 center, Color color, float radius = 10)
        {
            // our plane as a circle mesh
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            Vector3 p0 = p.ClosestPointOnPlane(Vector3.zero);
            Vector3 p1 = p.ClosestPointOnPlane(Camera.main.transform.up);
            // flip normal if its on the wrong side
            if (p.GetDistanceToPoint(Camera.main.transform.position) < 0)
            {
                p.SetNormalAndPosition(p.normal * -1, p0);
            }
            Vector3 planeVec = (p0 - p1).normalized;
            verts.Add(center);
            verts.Add(center + planeVec * radius);
            for (float i = 10; i <= 360; i += 10)
            {
                Quaternion q = Quaternion.AngleAxis(i, p.normal);
                Vector3 circleVec = q * planeVec;
                Vector3 newPnt = center + circleVec * radius;
                verts.Add(newPnt);
                tris.Add(0);
                tris.Add(verts.Count - 2);
                tris.Add(verts.Count - 1);
            }
            Mesh circleMesh = new Mesh
            {
                vertices = verts.ToArray(),
                triangles = tris.ToArray()
            };
            circleMesh.RecalculateNormals();
            if (circleMesh.vertexCount > 0)
            {
                Gizmos.color = color;
                Gizmos.DrawMesh(circleMesh);
            }
        }

#endif

        private void Update()
        {
            if (completed || strokes.Count == 0) return;
            UpdateInput();
            UpdateEvaluation();
        }

        public void Reset()
        {
            // game objects
            foreach (var strokePair in strokes.Values)
            {
                Destroy(strokePair.inpStroke.gameObject);
                Destroy(strokePair.refStroke.gameObject);
            }

            // state
            strokes.Clear();
            curStrokeIdx = 0;
            completed = false;
            pass = false;

            // data
            charData = null;
        }

        private void UpdateEvaluation()
        {
            // process current stroke
            if (curStroke.isValid)
            {
                EvaluateStroke(strokes[curStrokeIdx]);
                if (curStroke.strokeResult.pass)
                {
                    curStroke.inpStroke.gameObject.SetActive(false);
                    curStroke.refStroke.strokeRenderer.SetVisibility(true);
                    curStroke.refStroke.strokeRenderer.lineColor = config.completedColor;
                    curStroke.refStroke.strokeRenderer.SetHightlight(config.correctColor);
                    curStroke.refStroke.strokeRenderer.Highlight();
                    MoveToNextStroke();
                }
                else
                {
                    curStroke.inpStroke.gameObject.SetActive(false);
                    curStroke.refStroke.strokeRenderer.SetVisibility(true);
                    curStroke.refStroke.strokeRenderer.lineColor = config.wrongColor;
                    curStroke.refStroke.strokeRenderer.SetHightlight(config.wrongColor);
                    curStroke.refStroke.strokeRenderer.Highlight();
                    MoveToNextStroke();
                }
            };
            if (completed)
            {
                score = strokes.Count(sp => sp.Value.strokeResult.pass) / (float)strokes.Count;
                pass = score > 0;
                //Debug.Log(string.Format("{0} completed, pass: {1}, score: {2:0.00}", kanjiData.literal, pass, score));
                // update progress for the kanji
                if (score >= 1)
                {
                    charData.progress.flawlessClears++;
                    charData.progress.clears++;
                }
                else if (score > 0)
                {
                    charData.progress.clears++;
                }
            }
        }

        private DrawableStroke GenerateRefStroke(Stroke rawStroke, bool isHidden = false)
        {
            var refStroke = Instantiate(strokePrefab, transform).GetComponent<DrawableStroke>();
            refStroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
            refStroke.Init(this);
            refStroke.strokeRenderer.SetVisibility(!isHidden);
            refStroke.strokeRenderer.lineColor = config.hintColor;
            refStroke.strokeRenderer.lineWidth = config.thickness;
            refStroke.AddPoints(rawStroke.points);
            refStroke.Complete();
            return refStroke;
        }

        private DrawableStroke GenerateInpStroke()
        {
            // create the first input stroke
            var inputStroke = Instantiate(strokePrefab, transform).GetComponent<DrawableStroke>();
            inputStroke.gameObject.name = "Input stroke " + (curStrokeIdx + 1);
            inputStroke.Init(this);
            inputStroke.strokeRenderer.lineColor = config.drawnColor;
            inputStroke.strokeRenderer.lineWidth = config.thickness;
            inputStroke.gameObject.SetActive(false);
            return inputStroke;
        }

        private void MoveToNextStroke()
        {
            if (curStrokeIdx == (strokes.Count - 1))
            {
                completed = true;
                return;
            }
            else
            {
                curStrokeIdx++;
                strokes[curStrokeIdx].inpStroke.gameObject.SetActive(true);
            }
        }

        private void EvaluateStroke(StrokePair sp)
        {
            StrokeResult result = new StrokeResult();
            // all points need to be under the loose threshold
            result.pass = true;
            for (int i = 0; i < sp.inpStroke.refPoints.Count; i++)
            {
                float distance = Mathf.Abs((
                    sp.inpStroke.refPoints[i] -
                    sp.refStroke.refPoints[i]).magnitude);
                result.refPointDistances.Add(distance);
                result.pass &= distance < config.compThreshLoose;
            }
            // at least one point needs to be under the tight thresh
            float? tightDist = result.refPointDistances.FirstOrDefault(d => d < config.compThreshTight);
            if (tightDist != null)
            {
                result.tightPointIdx = result.refPointDistances.IndexOf(tightDist);
            }
            result.pass &= result.tightPointIdx != -1;
            // total length needs to be within limits
            float minVal = sp.refStroke.length - config.lengthBuffer;
            float maxVal = sp.refStroke.length + config.lengthBuffer;
            result.pass &= sp.inpStroke.length > minVal && sp.inpStroke.length < maxVal;
            sp.strokeResult = result;
        }

    }

}