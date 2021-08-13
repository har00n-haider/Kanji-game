    using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Deals with how the stroke data is visualised,
/// and some basic animations.
/// </summary>
public abstract class StrokeRenderer : MonoBehaviour
{
    [SerializeField]
    protected Material lineMaterial;

    // persistent line configuration
    public Color lineColor;
    public float lineWidth = 0.1f;

    public abstract void SetupLine();

    public abstract void SetVisibility(bool visibility);

    public abstract void UpdateLinePoints(List<Vector2> points);

    public abstract void ResetLineColor();

    #region Highlights

    protected struct HighlightData
    {
        public float fromWidth;
        public Color fromColor;
        public Coroutine co;
    }

    protected HighlightData highlightData;

    protected abstract void SetLineColor(Color color); 

    protected abstract void SetLineWidth(float width);

    protected abstract void ResetLineWidth();

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
