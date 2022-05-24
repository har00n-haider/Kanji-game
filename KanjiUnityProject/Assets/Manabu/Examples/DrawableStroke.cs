using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;

namespace Manabu.Examples
{


/// <summary>
/// Holds the data relevant to a kanji stroke. Uses 2D coordinates.
/// Designed to be used directly from the Kanji class.
/// 
/// All the poinst should be normalised to a 0-1 range.
/// </summary>
[RequireComponent(typeof(StrokeRenderer))]
public class DrawableStroke : MonoBehaviour
{
    // stats/data for the stroke
    public List<Vector2> refPoints; // key points in the stroke used for evaluation
    public List<Vector2> points;    // points used for visualising the line on screen
    private bool completed = false;
    public float length { get; protected set; }
    public bool isValid { get { return completed && refPoints?.Count == kanji.config.noRefPointsInStroke; } }  

    // refs
    public StrokeRenderer strokeRenderer { get; private set; }
    private DrawableCharacter kanji;

    public  void Awake()
    {
        strokeRenderer = GetComponent<StrokeRenderer>();
    }

    public  void Init(DrawableCharacter kanji)
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
        refPoints = SVGUtils.GenRefPntsForPnts(points, kanji.config.noRefPointsInStroke);
        length = SVGUtils.GetLengthForPnts(points);
        completed = true;
    }

    public void ClearLine()
    {
        points.Clear();
        strokeRenderer.UpdateLinePoints(points);
        completed = false;
    }

}

}