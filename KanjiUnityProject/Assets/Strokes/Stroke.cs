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

        line.startWidth = width;
        line.endWidth = width;

        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        Color startColor = color;
        Color endColor   = color;
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        line.colorGradient = gradient;
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
