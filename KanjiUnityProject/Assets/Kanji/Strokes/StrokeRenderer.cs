using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Deals with how the stroke data is visualised
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class StrokeRenderer : MonoBehaviour
{
    protected struct HighlightData
    {
        public float fromWidth;
        public Color fromColor;
        public Coroutine co;
    }
    protected HighlightData highlightData;

    // persistent line configuration
    public Color lineColor;
    public float lineWidth = 0.1f;
    [SerializeField]
    private Material lineMaterial;
    protected LineRenderer line;
    public bool visible { get { return line.enabled; } }

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public virtual void SetupLine()
    {
        line.useWorldSpace = false;
        line.material = lineMaterial;
        line.numCapVertices = 4;
        ResetLineWidth();
        ResetLineColor();
    }

    public void SetVisibility(bool visibility)
    {
        line.enabled = visibility;
    }

    public void UpdateLinePoints(List<Vector2> points)
    {
        line.positionCount = points.Count;
        line.SetPositions(points.ConvertAll(p => new Vector3(p.x, p.y)).ToArray());
    }

    public void ResetLineColor()
    {
        line.startColor = lineColor;
        line.endColor = lineColor;
    }

    public void SetLineColor(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }

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

    private void SetLineWidth(float width)
    {
        line.startWidth = width;
        line.endWidth = width;
    }

    private void ResetLineWidth()
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
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

}
