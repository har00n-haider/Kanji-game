using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class Kanji : MonoBehaviour
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
        public Stroke inpStroke = null;
        public Stroke refStroke = null;
        public StrokeResult strokeResult = null;
        public bool isValid { get { return inpStroke.isValid && refStroke.isValid; } }
    }

    // current state of the kanji
    protected Dictionary<int, StrokePair> strokes = new Dictionary<int, StrokePair>();
    protected int curStrokeIdx;
    public bool completed = false;
    public bool pass = false;
    public float score { get; private set; }
    protected StrokePair curStroke { get { return strokes[curStrokeIdx]; } }

    // kanji data
    public KanjiData kanjiData { get; private set; }
    protected ParsedKanjiData parsedKanjiData;

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

    // refs
    public Stroke strokePrefab;


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

    protected virtual void Update()
    {
        if (completed || strokes.Count == 0) return;
        UpdateInput();
        UpdateEvaluation();
    }

    public virtual void Init(KanjiData kanjiData)
    {
        // pull a kanji
        parsedKanjiData = KanjiSVGParser.GetStrokesFromSvg(kanjiData.svgContent);
        bool refKanjiHidden = kanjiData.progress.flawlessClears >= KanjiManager.hideReferenceThreshold;
        for (int sIdx = 0; sIdx < parsedKanjiData.strokes.Count; sIdx++)
        {
            // assuming we get these in order
            strokes.Add(
                sIdx,
                new StrokePair()
                {
                    refStroke = GenerateRefStroke(parsedKanjiData.strokes[sIdx], refKanjiHidden),
                    inpStroke = GenerateInpStroke()
                });
        }
        curStrokeIdx = 0;
        this.kanjiData = kanjiData;
        // start the looking for the first stroke
        strokes[0].inpStroke.gameObject.SetActive(true);
    }

    protected void UpdateEvaluation()
    {
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
            score = strokes.Count(sp => sp.Value.strokeResult.pass) / (float)strokes.Count;
            pass = score > 0;
            Debug.Log(string.Format("{0} completed, pass: {1}, score: {2:0.00}", kanjiData.literal, pass, score));
            // update progress for the kanji
            if (score >= 1)
            {
                kanjiData.progress.flawlessClears++;
                kanjiData.progress.clears++;
            }
            else if (score > 0)
            {
                kanjiData.progress.clears++;
            }
        }
    }

    private Stroke GenerateRefStroke(RawStroke rawStroke, bool isHidden = false)
    {
        var refStroke = Instantiate(strokePrefab, transform).GetComponent<Stroke>();
        refStroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
        refStroke.Init(this);
        refStroke.strokeRenderer.SetVisibility(!isHidden);
        refStroke.strokeRenderer.lineColor = hintColor;
        refStroke.AddPoints(rawStroke.points);
        refStroke.Complete();
        return refStroke;
    }

    private Stroke GenerateInpStroke()
    {
        // create the first input stroke 
        var inputStroke = Instantiate(strokePrefab, transform).GetComponent<Stroke>();
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

    protected void EvaluateStroke(StrokePair sp)
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
        if (tightDist != null)
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

    protected virtual void UpdateInput() {}

}

