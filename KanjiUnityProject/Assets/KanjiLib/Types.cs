﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


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

public class KanjiData 
{
    public enum CategoryType
    {
        kanjipower,
    }
    public string literal = string.Empty;
    public string code = string.Empty;
    public List<string> meanings = new List<string>();
    public List<string> readingsOn = new List<string>();
    public List<string> readingsKun = new List<string>();
    public string svgContent = string.Empty;
    public Tuple<CategoryType, string> category = null;
    public KanjiProgress progress = new KanjiProgress();
}

public class KanjiProgress 
{
    public int clears = 0;
    public int flawlessClears = 0;
}


public interface IKankiTraceable
{
    void Destroy();
}

public class PromptChar 
{
    public CharType type;
    public string character = "";
    public PromptChar()
    {
    }
    public PromptChar(string c, CharType t) 
    {
        character = c;
        type = t;
    }
    public static List<PromptChar> GetPromptListFromString(string origString, CharType stringType) 
    {
        List<PromptChar> result = new List<PromptChar>();
        foreach (var s in origString)
        {
            result.Add(new PromptChar(s.ToString(), stringType));
        }
        return result;
    }
}

public enum CharType
{
    Draw,
    Romaji,
    Hiragana,
    Katana,
}


