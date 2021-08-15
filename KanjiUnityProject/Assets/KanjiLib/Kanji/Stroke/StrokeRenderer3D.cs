using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
class StrokeRenderer3D : StrokeRenderer
{
    private LineRenderer line;

    public override void SetupLine()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.material = lineMaterial;
        line.numCapVertices = 4;
        ResetLineWidth();
        ResetLineColor();
    }

    public override void SetVisibility(bool visibility)
    {
        line.enabled = visibility;
    }

    public override void UpdateLinePoints(List<Vector2> points)
    {
        line.positionCount = points.Count;
        line.SetPositions(points.ConvertAll(p => new Vector3(p.x, p.y)).ToArray());
    }

    protected override void ResetLineColor()
    {
        line.startColor = lineColor;
        line.endColor = lineColor;
    }

    protected override void SetLineColorTemp(Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }

    protected override void SetLineWidthTemp(float width)
    {
        line.startWidth = width;
        line.endWidth = width;
    }

    protected override void ResetLineWidth()
    {
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
    }

}

