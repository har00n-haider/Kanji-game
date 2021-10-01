using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//TODO: mark this class as abstract
public class Kanji : MonoBehaviour
{
    #region classes

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

    [Serializable]
    public class KanjiConfig
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

    #endregion classes

    // current state of the kanji
    protected Dictionary<int, StrokePair> strokes = new Dictionary<int, StrokePair>();

    protected StrokePair curStroke { get { return strokes[curStrokeIdx]; } }
    protected int curStrokeIdx = 0;
    public bool completed { get; private set; } = false;
    public bool pass { get; private set; } = false;
    public float score { get; private set; } = 0;

    // kanji data
    [SerializeField]
    private KanjiConfig _config;

    public KanjiConfig config { get { return _config; } private set { _config = value; } }
    public KanjiData kanjiData { get; private set; } = null;
    protected ParsedKanjiData parsedKanjiData = null;

    // refs
    public Stroke strokePrefab;

    protected virtual void Update()
    {
        if (completed || strokes.Count == 0) return;
        UpdateInput();
        UpdateEvaluation();
    }

    /// <summary>
    /// Handles the loading of a kanji from the parser
    /// </summary>
    /// <param name="kanjiData"></param>
    /// <param name="scale"></param>
    public virtual void Init(KanjiData kanjiData)
    {
        // pull a kanji
        parsedKanjiData = KanjiSVGParser.GetStrokesFromSvg(kanjiData.svgContent);
        bool refKanjiHidden = kanjiData.progress.flawlessClears >= KanjiManager.hideWritingRefThreshold;
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

    public virtual void Reset()
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
        kanjiData = null;
        parsedKanjiData = null;
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
                kanjiData.progress.flawlessClears++;
                kanjiData.progress.clears++;
            }
            else if (score > 0)
            {
                kanjiData.progress.clears++;
            }
            Completed();
        }
    }

    private Stroke GenerateRefStroke(RawStroke rawStroke, bool isHidden = false)
    {
        var refStroke = Instantiate(strokePrefab, transform).GetComponent<Stroke>();
        refStroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
        refStroke.Init(this);
        refStroke.strokeRenderer.SetVisibility(!isHidden);
        refStroke.strokeRenderer.lineColor = config.hintColor;
        refStroke.strokeRenderer.lineWidth = config.thickness;
        refStroke.AddPoints(rawStroke.points);
        refStroke.Complete();
        return refStroke;
    }

    private Stroke GenerateInpStroke()
    {
        // create the first input stroke
        var inputStroke = Instantiate(strokePrefab, transform).GetComponent<Stroke>();
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

    /// <summary>
    /// The input has to be provided in normalised coordinates (0-1) in this coordinate system:
    ///  y/\
    ///   |
    ///   |
    ///    -----> x
    /// relative to a rect in which the kanji exists
    /// </summary>
    protected virtual void UpdateInput() { }

    protected virtual void Completed()
    {
    }
}