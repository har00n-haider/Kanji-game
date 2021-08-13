using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StrokeRenderer))]
public class Stroke : MonoBehaviour
{
    // stats/data for the stroke
    public List<Vector2> refPoints; // key points in the stroke used for evaluation
    public List<Vector2> points;    // points used for visualising the line on screen
    public Kanji kanji;
    public bool completed = false;
    public float length { get; protected set; }

    public bool isValid { get { return completed && refPoints?.Count == kanji.noRefPointsInStroke; } }
  
    public StrokeRenderer strokeRenderer;

    public virtual void Awake()
    {
        strokeRenderer = GetComponent<StrokeRenderer>();
    }

    public virtual void Init(Kanji kanji)
    {
        this.kanji = kanji;
    }
        
}
