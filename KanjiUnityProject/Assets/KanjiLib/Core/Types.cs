using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace KanjiLib.Core
{

public class ParsedKanjiData
{
    public float width;
    public float height;
    public float scale;
    public float centerXOffset;
    public float centerYOffset;
    public float scaledWidth;
    public float scaledHeight;
    public List<RawStroke> strokes = new List<RawStroke>();

    public ParsedKanjiData(float width, float height, float scale, List<RawStroke> strokes = null)
    {
        this.width = width;
        this.height = height;
        this.scale = scale;
        centerXOffset = width / 2;
        centerYOffset = height / 2;
        scaledWidth = width * scale;
        scaledHeight = height * scale;
        this.strokes = strokes;
    }
}

public class RawStroke
{
    public int orderNo;
    public List<Vector2> points = new List<Vector2>();
}

/// <summary>
/// Holds all data relevant to a given kanji
/// Required by the kanji draw classes
/// </summary>
public class KanjiData
{
    public string literal = string.Empty;
    public string code = string.Empty;
    public List<string> meanings = new List<string>();
    public List<string> readingsOn = new List<string>();
    public List<string> readingsKun = new List<string>();
    public string svgContent = string.Empty;
    public string categoryType = string.Empty;
    public string category = string.Empty;
    public KanjiProgress progress = new KanjiProgress();
}

public class KanjiProgress
{
    public int clears = 0;
    public int flawlessClears = 0;
}


}