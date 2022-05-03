using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace KanjiLib.Draw
{

/// <summary>
/// Deals with how the stroke data is visualised,
/// and some basic animations.
/// </summary>
public abstract class StrokeRenderer : MonoBehaviour
{
    [SerializeField]
    protected Material lineMaterial;

    // persistent line configuration
    protected Color _lineColor = Color.red;
    public Color lineColor { get { return _lineColor; } set { _lineColor = value;  ResetLineColor(); }  }
    protected float _lineWidth = 0.1f;
    public float lineWidth { get { return _lineWidth; } set { _lineWidth = value; ResetLineWidth(); } }

    public abstract void SetupLine();

    public abstract void SetVisibility(bool visibility);

    public abstract void UpdateLinePoints(List<Vector2> points);


    #region Highlights

    protected struct HighlightData
    {
        public float fromWidth;
        public Color fromColor;
        public Coroutine co;
    }

    protected HighlightData highlightData;

    protected abstract void SetLineColorTemp(Color color); 

    protected abstract void SetLineWidthTemp(float width);

    protected abstract void ResetLineWidth();

    protected abstract void ResetLineColor();

    public void SetHightlight(Color color, float widthScale = 2)
    {
        highlightData.fromColor = color;
        highlightData.fromWidth = lineWidth*widthScale;
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
            SetLineColorTemp(Color.Lerp(highlightData.fromColor, lineColor, highlightAlpha));
            SetLineWidthTemp(Mathf.Lerp(highlightData.fromWidth, lineWidth, highlightAlpha));
            yield return new WaitForSeconds(0.01f);
        }
        ResetLineColor();
        ResetLineWidth();
    }

    #endregion

}

}