using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KanjiLib.Draw
{

[RequireComponent(typeof(LineRenderer))]
class StrokeRenderer3D : StrokeRenderer
{
    private LineRenderer line;
    private Kanji3D kanji3D;

    public override void SetupLine()
    {
        // hierarchy dependant
        kanji3D = GetComponentInParent<Kanji3D>();
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

}