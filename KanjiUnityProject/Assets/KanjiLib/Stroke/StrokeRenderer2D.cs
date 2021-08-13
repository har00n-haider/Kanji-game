using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(UILineRenderer))]
class StrokeRenderer2D : StrokeRenderer
{
    private UILineRenderer line;

    public override void SetupLine()
    {
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
        line.Points = points.ToArray();
    }

    public override void ResetLineColor()
    {
        line.color = lineColor;
    }

    protected override void ResetLineWidth()
    {
        line.LineThickness = lineWidth;
    }

    protected override void SetLineColor(Color color)
    {
        line.color = color;
    }

    protected override void SetLineWidth(float width)
    {
        line.LineThickness = width;
    }
}
