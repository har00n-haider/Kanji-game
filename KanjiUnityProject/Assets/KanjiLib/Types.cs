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

// Interface throught which the Prompt holder can control the main script
// of a given game object that interacts with the prompt system

public interface IPromptHolderControllable
{
    void Destroy();
}

/// <summary>
/// What is displayed in the prompt.
/// Limited by word type
/// </summary>
public enum PromptType
{
    Kanji,
    Romaji,
    Hiragana,
    Katana,
    Meaning
}

/// <summary>
/// What the input needs to supply (Call/Response type).
/// Limited by word type
/// </summary>
public enum InputType
{
    KeyHiragana,
    KeyKatakana,
    KeyHiraganaWithRomaji,
    KeyKatakanaWithRomaji,
    WritingHiragana,
    WritingKatakana,
    WritingKanji,
    Meaning,
}

public class PromptChar
{
    public char character = ' ';
    public KanjiData data = null;
}

[System.Serializable]
public class PromptWord
{
    /// <summary>
    /// Cassification of the word. Can only perform
    /// certain call/responses for certain word types
    /// </summary>
    public enum WordType
    {
        kanji = 1,
        hiragana = 2,
        katakana = 3
    }

    /// <summary>
    /// Classification of the word
    /// </summary>
    public WordType type;

    public string kanji;
    public string[] meanings;
    public string romaji;
    public string hiragana;
    public string katakana;

    /// <summary>
    /// Type of response required to complete the word
    /// </summary>
    [System.NonSerialized]
    public InputType responseType;

    /// <summary>
    /// Determines the way the word will be displayed on a prompt
    /// </summary>
    [System.NonSerialized]
    public PromptType displayType;

    /// <summary>
    /// These are iterated through by the input to complete a word. Not used for display
    /// </summary>
    [System.NonSerialized]
    public PromptChar[] chars;

    // state
    [System.NonSerialized]
    private int cIdx = 0;

    private bool meaningCompleted = false;
    private static readonly char fillerChar = '☐';

    public override string ToString()
    {
        string s = string.Empty;
        foreach (var x in chars)
        {
            s += x.character;
        }
        return s;
    }

    public void Reset()
    {
        cIdx = 0;
        meaningCompleted = false;
    }

    public bool Completed()
    {
        switch (responseType)
        {
            case InputType.KeyHiragana:
            case InputType.KeyKatakana:
            case InputType.KeyHiraganaWithRomaji:
            case InputType.KeyKatakanaWithRomaji:
            case InputType.WritingHiragana:
            case InputType.WritingKatakana:
            case InputType.WritingKanji:
                return cIdx == chars.Length;

            case InputType.Meaning:
                return meaningCompleted;

            default:
                return false;
        }
    }

    #region Meaning tracking

    public string GetMeaning()
    {
        return meanings.Length > 0 ? meanings[0] : "-----";
    }

    public bool CheckMeaning(string input)
    {
        meaningCompleted = input == GetMeaning();
        return meaningCompleted;
    }

    #endregion Meaning tracking

    #region Input tracking progress

    public string GetCompletedKanaString()
    {
        string s = string.Empty;
        for (int i = 0; i < katakana.Length; i++)
        {
            s += i < cIdx ? katakana[i].ToString() : string.Empty;
        }
        return s;
    }

    public string GetDisplayString()
    {
        string s = string.Empty;
        for (int i = 0; i < chars.Length; i++)
        {
            s += i < cIdx ? chars[i].character : fillerChar;
        }
        return s;
    }

    public PromptChar GetChar()
    {
        return chars[cIdx];
    }

    public bool CheckChar(char c)
    {
        if (c == chars[cIdx].character)
        {
            ++cIdx;
            return true;
        }
        else
        {
            return false;
        }
    }

    #endregion Input tracking progress
}

[System.Serializable]
public class Prompt
{
    public List<PromptWord> words;

    public override string ToString()
    {
        string promptStr = "";
        foreach (var word in words)
        {
            promptStr += word.ToString();
        }
        return promptStr;
    }
}

[System.Serializable]
public class PromptList
{
    public List<Prompt> sentences;
}