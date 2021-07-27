using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Will take the input and draw stroke and generate key points
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class Stroke : MonoBehaviour
{

    // on the in plane 
    public List<Vector2> refPoints;
    public Kanji kanji;
    public bool completed = false;
    // line stuff
    public Material lineMaterial;
    public float width = 0.1f;
    public LineRenderer line;


    protected virtual void SetupLine(Color color) 
    {
        line.material = lineMaterial;

        line.numCapVertices = 4;

        SetLineWidth(width);

        SetLineColor(color);
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

}
