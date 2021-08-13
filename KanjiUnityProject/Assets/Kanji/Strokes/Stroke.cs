﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the data relevant to a kanji stroke. Uses 2D coordinates.
/// </summary>
[RequireComponent(typeof(StrokeRenderer))]
public class Stroke : MonoBehaviour
{
    // stats/data for the stroke
    public List<Vector2> refPoints; // key points in the stroke used for evaluation
    public List<Vector2> points;    // points used for visualising the line on screen
    private bool completed = false;
    public float length { get; protected set; }
    public bool isValid { get { return completed && refPoints?.Count == kanji.noRefPointsInStroke; } }  

    // refs
    public StrokeRenderer strokeRenderer { get; private set; }
    private Kanji kanji;

    public virtual void Awake()
    {
        strokeRenderer = GetComponent<StrokeRenderer>();
    }

    public virtual void Init(Kanji kanji)
    {
        this.kanji = kanji;
        strokeRenderer.SetupLine();
    }
    public void AddPoint(Vector2 point)
    {
        points.Add(point);
        strokeRenderer.UpdateLinePoints(points);
    }

    public void AddPoints(List<Vector2> newPoints)
    {
        points.AddRange(newPoints);
        strokeRenderer.UpdateLinePoints(points);
    }

    public void Complete()
    {
        refPoints = KanjiUtils.GenRefPntsForPnts(points, kanji.noRefPointsInStroke);
        length = KanjiUtils.GetLengthForPnts(points);
        completed = true;
    }

    public void ClearLine()
    {
        points.Clear();
        strokeRenderer.UpdateLinePoints(points);
        completed = false;
    }

}
