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
        public float initialWidth;
        public float finalWidth;
        public Color initialColor;
        public Color finalColor;
        public Coroutine co;
    }
    protected HighlightData highlightData;

    // kanji related stuff
    public List<Vector2> refPoints; // key points in the stroke used for evaluation
    public List<Vector2> points;    // points used for visualising the line on screen
    public Kanji kanji;
    public bool completed = false;
    public float length { get; protected set; }

    // line stuff
    public Material lineMaterial;
    public float width = 0.1f;
    public LineRenderer line;

    public bool isValid { get { return completed && refPoints?.Count == kanji.noRefPointsInStroke; } }
  
    protected virtual void SetupLine(Color color) 
    {
        line.material = lineMaterial;
        line.numCapVertices = 4;
        SetLineWidth(width);
        SetLineColor(color);
        // highlight configuration
        highlightData.finalColor = line.startColor;
        highlightData.finalWidth = line.startWidth;
    }

    protected void SetLinePoints()
    {
        line.positionCount = points.Count;
        line.SetPositions(points.ConvertAll(p => new Vector3(p.x, p.y)).ToArray());
    }

    protected void SetLineColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }

    protected void SetLineWidth (float width)
    {
        line.startWidth = width;
        line.endWidth = width;
    }

    public virtual void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public virtual void Init(Kanji kanji)
    {
        this.kanji = kanji;
    }


        
    #region Highlight

    public void Highlight()
    {
        if (highlightData.co != null) StopCoroutine(highlightData.co);
        SetLineColor(highlightData.finalColor);
        SetLineWidth(highlightData.finalWidth); 
        highlightData.co = StartCoroutine(ApplyHighlight());
    }

    private IEnumerator ApplyHighlight()
    {
        //Debug.Log("Running highlight coroutine");
        for (float highlightAlpha = 0; highlightAlpha <= 1; highlightAlpha += 0.05f)
        {
            SetLineColor(Color.Lerp(highlightData.initialColor, highlightData.finalColor, highlightAlpha));
            SetLineWidth(Mathf.Lerp(highlightData.initialWidth, highlightData.finalWidth, highlightAlpha));
            yield return new WaitForSeconds(0.01f);
        }
    }

    #endregion

}
