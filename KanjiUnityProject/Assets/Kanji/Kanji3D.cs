using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Kanji3D : Kanji
{

    // current state of the kanji
    private Dictionary<int, StrokePair> strokes = new Dictionary<int, StrokePair>();
    private int curStrokeIdx;
    public bool completed = false;
    public bool pass = false;
    public float score { get; private set; }
    private StrokePair curStroke { get { return strokes[curStrokeIdx]; } }

    // configuration for strokes
    public float compThreshTight = 0.3f;
    public float compThreshLoose = 0.7f;
    public float lengthBuffer = 1f;
    public Color wrongColor;
    public Color correctColor;
    public Color hintColor;
    public Color completedColor;
    public Color drawnColor;

    public KanjiData kanjiData { get; private set; }

    public ReferenceStroke refStrokePrefab;
    public InputStroke inpStrokePrefab;

    private KanjiGrid kanjiGrid;

#if UNITY_EDITOR
    // debug - set these in the editor
    public bool debug = false;
    public KanjiManager kanjiManager;
    public char debugChar = '一';
#endif

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_EDITOR
        if (debug)
        {
            Init(kanjiManager.database.GetKanji(debugChar));
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (completed || strokes.Count == 0) return;
        UpdateInput();
        // process current stroke
        if (curStroke.isValid)
        {
            EvaluateStroke(strokes[curStrokeIdx]);
            if (curStroke.strokeResult.pass)
            {
                curStroke.inpStroke.gameObject.SetActive(false);
                curStroke.refStroke.strokeRenderer.SetVisibility(true);
                curStroke.refStroke.strokeRenderer.lineColor = completedColor;
                curStroke.refStroke.strokeRenderer.SetHightlight(correctColor);
                curStroke.refStroke.strokeRenderer.Highlight();
                MoveToNextStroke();
            }
            else
            {
                curStroke.inpStroke.gameObject.SetActive(false);
                curStroke.refStroke.strokeRenderer.SetVisibility(true);
                curStroke.refStroke.strokeRenderer.lineColor = wrongColor;
                curStroke.refStroke.strokeRenderer.SetHightlight(wrongColor);
                curStroke.refStroke.strokeRenderer.Highlight();
                MoveToNextStroke();
            }
        };
        if (completed) 
        {
            score = strokes.Count(sp => sp.Value.strokeResult.pass) / (float) strokes.Count;
            pass = score > 0;
            Debug.Log(string.Format("{0} completed, pass: {1}, score: {2:0.00}", kanjiData.literal, pass, score));
            // update progress for the kanji
            if (score >= 1) 
            {
                kanjiData.progress.flawlessClears++;
                kanjiData.progress.clears++;
            }
            else if( score > 0)
            {
                kanjiData.progress.clears++;
            }
        }
    }

    public void Init(KanjiData kanjiData)
    {
        // pull a kanji
        ParsedKanjiData parsedKanji = KanjiSVGParser.GetStrokesFromSvg(kanjiData.svgContent);
        bool refKanjiHidden = kanjiData.progress.flawlessClears >= KanjiManager.hideReferenceThreshold;
        for (int sIdx = 0; sIdx < parsedKanji.strokes.Count; sIdx++)
        {
            // assuming we get these in order
            strokes.Add(
                sIdx,
                new StrokePair()
                {
                    refStroke = GenerateRefStroke(parsedKanji.strokes[sIdx], refKanjiHidden),
                    inpStroke = GenerateInpStroke()
                });
        }
        curStrokeIdx = 0;
        this.kanjiData = kanjiData;
        // setup the grid
        kanjiGrid = GetComponentInChildren<KanjiGrid>();
        kanjiGrid.Init(parsedKanji);
        // start the looking for the first stroke
        strokes[0].inpStroke.gameObject.SetActive(true);
    }

    private ReferenceStroke GenerateRefStroke(RawStroke rawStroke, bool isHidden = false)
    {
        var refStroke = Instantiate(refStrokePrefab, transform).GetComponent<ReferenceStroke>();
        refStroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
        refStroke.strokeRenderer.SetVisibility(!isHidden);
        refStroke.strokeRenderer.lineColor = hintColor;
        refStroke.rawStroke = rawStroke;
        refStroke.Init(this);
        return refStroke;
    }

    private InputStroke GenerateInpStroke()
    {
        // create the first input stroke 
        var inputStroke = Instantiate(inpStrokePrefab, transform).GetComponent<InputStroke>();
        inputStroke.gameObject.name = "Input stroke " + (curStrokeIdx + 1);
        inputStroke.strokeRenderer.lineColor = drawnColor;
        inputStroke.Init(this);
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
            result.pass &= distance < compThreshLoose;
        }
        // at least one point needs to be under the tight thresh
        float? tightDist = result.refPointDistances.FirstOrDefault(d => d < compThreshTight);
        if(tightDist != null) 
        {
            result.tightPointIdx = result.refPointDistances.IndexOf(tightDist); 
        }
        result.pass &= result.tightPointIdx != -1;
        // total length needs to be within limits
        float minVal = sp.refStroke.length - lengthBuffer;
        float maxVal = sp.refStroke.length + lengthBuffer;
        result.pass &= sp.inpStroke.length > minVal && sp.inpStroke.length < maxVal;
        sp.strokeResult = result;
    }

    private void UpdateInput()
    {
        //TODO: move the input stuff to Kanji
        // populate line
        if (Input.GetMouseButton(0))
        {
            // convert mouse position to a point on the kanji plane 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool hit = GetPlane().Raycast(ray, out float enter);
            if (hit)
            {
                Vector3 worldPoint = ray.direction * enter + ray.origin;
                Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                curStroke.inpStroke.AddPoint(localPoint);
            }
        }
        // clear line
        if (Input.GetMouseButtonUp(0))
        {
            curStroke.inpStroke.Complete();
        }
    }

    // Get the plane on which the the 3d kanji lies
    private Plane GetPlane()
    {
        // create the plane on which the kanji will be drawn
        Vector3 planePoint = gameObject.transform.position;
        Vector3 planeDir = -gameObject.transform.forward;
        return new Plane(planeDir.normalized, planePoint);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debug) return;
        // plane
        Plane kanjiPlane = GetPlane();
        DrawPlane(kanjiPlane, kanjiPlane.ClosestPointOnPlane(transform.position), new Color(0, 0, 1, 0.1f));

        // draw debug strokes
        if (strokes.Count > 0)
        {
            for (int i = 0; i <= curStrokeIdx; i++)
            {
                if(strokes[i].strokeResult != null) DrawStrokePair(strokes[i]);
            }
        }
    }

    private void DrawStrokePair(StrokePair sp)
    {
        if (sp.isValid)
        {
            for (int i = 0; i < noRefPointsInStroke; i++)
            {
                Gizmos.color = Color.gray;
                var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.refPoints[i].x, sp.refStroke.refPoints[i].y));
                Gizmos.DrawSphere(refPnt, 0.1f);
                Gizmos.color = new Color(0,0,0,0.1f);
                Gizmos.DrawSphere(refPnt, compThreshLoose);
                Gizmos.DrawSphere(refPnt, compThreshTight);
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
}
