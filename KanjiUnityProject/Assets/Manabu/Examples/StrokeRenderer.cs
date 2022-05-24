using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Manabu.Examples
{

[RequireComponent(typeof(LineRenderer))]
public class StrokeRenderer : MonoBehaviour
{
    private LineRenderer line;
    private DrawableCharacter kanji3D;

    public  void SetupLine()
    {
        // hierarchy dependant
        kanji3D = GetComponentInParent<DrawableCharacter>();
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.material = lineMaterial;
        line.numCapVertices = 4;
        ResetLineWidth();
        ResetLineColor();
    }

    public  void SetVisibility(bool visibility)
    {
        line.enabled = visibility;
    }

    public  void UpdateLinePoints(List<Vector2> points)
    {
        if (kanji3D.boxCollider == null) return;

        line.positionCount = points.Count;
        // set the normalised points to the size of the collider
        Vector3[] scaledPoints = new Vector3[points.Count];
        for (int i = 0; i < scaledPoints.Length; i++)
        {
            scaledPoints[i] = new Vector3(
                points[i].x,
                points[i].y,
                // center of the box
                kanji3D.kanjiZBoxRelativepos);
            scaledPoints[i].Scale(kanji3D.boxCollider.size);
        }
        line.SetPositions(scaledPoints);
    }

    protected  void ResetLineColor()
    {
        line.startColor = lineColor;
        line.endColor = lineColor;
    }

    protected  void SetLineColorTemp(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }

    protected  void SetLineWidthTemp(float width)
    {
        line.startWidth = width;
        line.endWidth = width;
    }

    protected  void ResetLineWidth()
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
    }

        [SerializeField]
    protected Material lineMaterial;

    // persistent line configuration
    protected Color _lineColor = Color.red;
    public Color lineColor { get { return _lineColor; } set { _lineColor = value;  ResetLineColor(); }  }
    protected float _lineWidth = 0.1f;
    public float lineWidth { get { return _lineWidth; } set { _lineWidth = value; ResetLineWidth(); } }



    #region Highlights

    protected struct HighlightData
    {
        public float fromWidth;
        public Color fromColor;
        public Coroutine co;
    }

    protected HighlightData highlightData;


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