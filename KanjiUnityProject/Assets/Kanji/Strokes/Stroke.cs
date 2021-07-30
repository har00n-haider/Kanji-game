using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Will take the input and draw stroke and generate key points
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class Stroke : MonoBehaviour
{
    protected struct HighlightData
    {
        public float fromWidth;
        public Color fromColor;
        public Coroutine co;
    }
    protected HighlightData highlightData;

    // stats/data for the stroke
    public List<Vector2> refPoints; // key points in the stroke used for evaluation
    public List<Vector2> points;    // points used for visualising the line on screen
    public Kanji kanji;
    public bool completed = false;
    public float length { get; protected set; }
    public bool visible { get { return line.enabled; } }

    // persistent line configuration
    public Color lineColor;
    public float lineWidth = 0.1f;
    [SerializeField]
    private Material lineMaterial;
    protected LineRenderer line;

    public bool isValid { get { return completed && refPoints?.Count == kanji.noRefPointsInStroke; } }
  
    protected virtual void SetupLine() 
    {
        line.material = lineMaterial;
        line.numCapVertices = 4;
        ResetLineWidth();
        ResetLineColor();
    }

    protected void UpdateLinePoints()
    {
        line.positionCount = points.Count;
        line.SetPositions(points.ConvertAll(p => new Vector3(p.x, p.y)).ToArray());
    }

    public void ResetLineColor()
    {
        line.startColor = lineColor;
        line.endColor = lineColor;
    }

    private void ResetLineWidth ()
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
    }

    public void SetLineColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }

    private void SetLineWidth(float width)
    {
        line.startWidth = width;
        line.endWidth = width;
    }

    public virtual void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void SetVisibility(bool visibility) 
    {
        line.enabled = visibility;
    }

    public virtual void Init(Kanji kanji)
    {
        this.kanji = kanji;
    }
        
    #region Highlight

    public void SetHightlight(Color color, float width = 0.3f) 
    {
        highlightData.fromColor = color;
        highlightData.fromWidth = width;
    }

    public void Highlight()
    {
        if (highlightData.co != null) StopCoroutine(highlightData.co);
        // reset the state of the line
        ResetLineColor();
        ResetLineWidth(); 
        highlightData.co = StartCoroutine(ApplyHighlight());
    }

    private IEnumerator ApplyHighlight()
    {
        //Debug.Log("Running highlight coroutine");
        for (float highlightAlpha = 0; highlightAlpha <= 1; highlightAlpha += 0.05f)
        {
            SetLineColor(Color.Lerp(highlightData.fromColor, lineColor, highlightAlpha));
            SetLineWidth(Mathf.Lerp(highlightData.fromWidth, lineWidth, highlightAlpha));
            yield return new WaitForSeconds(0.01f);
        }
        ResetLineColor();
        ResetLineWidth();
    }

    #endregion

}
