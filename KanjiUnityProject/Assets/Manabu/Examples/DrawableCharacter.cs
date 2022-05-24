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
        public DrawConfiguration config = null;

        public void EvaluateStroke()
        {
            StrokeResult result = new StrokeResult();
            // all points need to be under the loose threshold
            result.pass = true;
            for (int i = 0; i < inpStroke.refPoints.Count; i++)
            {
                float distance = Mathf.Abs((
                    inpStroke.refPoints[i] -
                    refStroke.refPoints[i]).magnitude);
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
            float minVal = refStroke.length - config.lengthBuffer;
            float maxVal = refStroke.length + config.lengthBuffer;
            result.pass &= inpStroke.length > minVal && inpStroke.length < maxVal;
            strokeResult = result;
        }
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
        // box resize 
        // used to scale the normalised kanji points to the the dims of the box
        [HideInInspector]
        public BoxCollider boxCollider = null;
        [SerializeField]
        private Vector3 boxColliderSize; // width/height size
        [SerializeField]
        public float kanjiZBoxRelativepos = 0.5f;

        // current state of the kanji
        private Dictionary<int, StrokePair> strokes = new Dictionary<int, StrokePair>();
        private StrokePair curStroke { get { return strokes[curStrokeIdx]; } }
        private int curStrokeIdx = 0;
        public bool completed { get; private set; } = false;

        // character data
        [SerializeField]
        private DrawConfiguration _config;
        public DrawConfiguration config { get { return _config; } private set { _config = value; } }
        public Character Character { get; private set; } = null;
        [SerializeField]
        private char character;
        [SerializeField]
        private TextAsset databaseFile;

        // refs
        public DrawableStroke strokePrefab;
        private CharacterGrid charGrid = null;

        // events
        public event Action OnCompleted;

        private void Start()
        {
            // would do this somewhere else (management object etc.) , but done here for the example
            Database database = new Database();
            database.Load(databaseFile);
            Character = database.GetCharacter(character);
            Init(Character);
        }

        private void Init(Character charData)
        {
            // need the collider set be set up to normalise input for comparison
            if (boxCollider == null) boxCollider = GetComponent<BoxCollider>(); 
            ResizeCollider();

            // setup the grid
            if (charGrid == null) charGrid = GetComponentInChildren<CharacterGrid>();
            charGrid.Init(this);

            // generate the pairs of strokes that will take the input points, one stroke at a time
            for (int strokeIdx = 0; strokeIdx < charData.drawData.strokes.Count; strokeIdx++)
            {
                // assuming we get these in order
                strokes.Add(
                    strokeIdx,
                    new StrokePair()
                    {
                        refStroke = GenerateRefStroke(charData.drawData.strokes[strokeIdx]),
                        inpStroke = GenerateInpStroke(),
                        config = config
                    });
            }

            curStrokeIdx = 0;
            this.Character = charData;
            // start the looking for the first stroke
            strokes[0].inpStroke.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (completed || strokes.Count == 0) return;
            UpdateInput();

            // process current stroke
            if (curStroke.isValid)
            {
                strokes[curStrokeIdx].EvaluateStroke();
                if (curStroke.strokeResult.pass)
                {
                    curStroke.refStroke.lineColor = config.completedColor;
                    curStroke.refStroke.SetHightlight(config.correctColor);
                }
                else
                {
                    curStroke.refStroke.lineColor = config.wrongColor;
                    curStroke.refStroke.SetHightlight(config.wrongColor);
                }
                curStroke.inpStroke.gameObject.SetActive(false);
                curStroke.refStroke.SetVisibility(true);
                curStroke.refStroke.Highlight();
                MoveToNextStroke();
            };

            // deal with a completed character
            if (completed) OnCompleted?.Invoke();
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

        private DrawableStroke GenerateRefStroke(Stroke rawStroke, bool isHidden = false)
        {
            var refStroke = Instantiate(strokePrefab, transform).GetComponent<DrawableStroke>();
            refStroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
            refStroke.Init(this);
            refStroke.SetVisibility(!isHidden);
            refStroke.lineColor = config.hintColor;
            refStroke.lineWidth = config.thickness;
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
            inputStroke.lineColor = config.drawnColor;
            inputStroke.lineWidth = config.thickness;
            inputStroke.gameObject.SetActive(false);
            return inputStroke;
        }

        // Get the plane on which the the 3d kanji lies
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

            // data
            Character = null;
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

        private void ResizeCollider()
        {
            Vector3 halfSize = boxColliderSize / 2;
            boxCollider.size = boxColliderSize;
            boxCollider.center = halfSize;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
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
                    float radius = boxCollider.size.magnitude / 100.0f;

                    Gizmos.color = Color.gray;
                    var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.refPoints[i].x, sp.refStroke.refPoints[i].y, kanjiZBoxRelativepos));
                    Gizmos.DrawSphere(refPnt, radius);
                    Gizmos.color = new Color(0, 0, 0, 0.1f);
                    Gizmos.DrawSphere(refPnt, config.compThreshLoose);
                    Gizmos.DrawSphere(refPnt, config.compThreshTight);
                    Gizmos.color = sp.strokeResult.pass ? Color.green : Color.red;
                    // tight dist color
                    Gizmos.color = sp.strokeResult.tightPointIdx == i ? new Color(1, 0, 1) : Gizmos.color; // purple
                    var inpPnt = transform.TransformPoint(new Vector3(sp.inpStroke.refPoints[i].x, sp.inpStroke.refPoints[i].y, kanjiZBoxRelativepos));
                    Gizmos.DrawSphere(inpPnt, radius);
                    // connect the two
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(refPnt, inpPnt);
                }
            }
        }

#endif

    }
}