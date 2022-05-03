using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI.Extensions;


namespace KanjiLib.Draw
{

[RequireComponent(typeof(UILineRenderer))]
class StrokeRenderer2D : StrokeRenderer
{
    private UILineRenderer line;
    private BoxCollider2D boxCollider;
    private RectTransform strokeRectT;
    private RectTransform kanjiRectT;

    public override void SetupLine()
    {
        //TODO: maybe pass these in as arguments to the stroke/stroke renderer?
        kanjiRectT = GetComponentInParent<RectTransform>();
        strokeRectT = GetComponent<RectTransform>();
        UIUtils.StretchToParentSize(strokeRectT, kanjiRectT, Vector2.zero);

        boxCollider = GetComponentInParent<BoxCollider2D>();
        line = GetComponent<UILineRenderer>();
        line.material = lineMaterial;
        ResetLineWidth();
        ResetLineColor();
    }

    public override void SetVisibility(bool visibility)
    {
        line.enabled = visibility;
    }

    public override void UpdateLinePoints(List<Vector2> points)
    {
        Vector2[] scaledPoints = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            scaledPoints[i] = points[i];
            scaledPoints[i].Scale(boxCollider.size);
        }
        line.Points = scaledPoints;
    }

    protected override void ResetLineColor()
    {
        line.color = lineColor;
    }

    protected override void ResetLineWidth()
    {
        line.LineThickness = lineWidth;
    }

    protected override void SetLineColorTemp(Color color)
    {
        line.color = color;
    }

    protected override void SetLineWidthTemp(float width)
    {
        line.LineThickness = width;
    }
}

}