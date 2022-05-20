using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Manabu.Core
{

public class DrawData
{
    public float width;
    public float height;
    public float scale;
    public float centerXOffset;
    public float centerYOffset;
    public float scaledWidth;
    public float scaledHeight;
    public List<Stroke> strokes = new List<Stroke>();

    public DrawData(float width, float height, float scale, List<Stroke> strokes = null)
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

public class Stroke
{
    public int orderNo;
    public List<Vector2> points = new List<Vector2>();
}

/// <summary>
/// Holds all data relevant to a given japanese charater. This is 
/// provided by the KanjiDatabase functions
/// </summary>
public class CharacterData
{
    public string literal = string.Empty;
    public string code = string.Empty;
    public List<string> meanings = new List<string>();
    public List<string> readingsOn = new List<string>();
    public List<string> readingsKun = new List<string>();
    public string svgContent = string.Empty; // TODO: Move the parsing of this earlier in the chain (i.e. in the KanjiDatabase)
    public string categoryType = string.Empty;
    public string category = string.Empty;
    public CharacterProgress progress = new CharacterProgress();
}

public class CharacterProgress
{
    public int clears = 0;
    public int flawlessClears = 0;
}

public enum CharacterType
{
    none = -1,
    kanji = 1,
    hiragana = 2,
    katakana = 3
}

/// <summary>
/// What is displayed in the prompt.
/// Limited by word type
/// </summary>
public enum DisplayType
{
    Kanji,
    Romaji,
    Hiragana,
    Katana,
    Meaning,
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


public class Character
{
    /// <summary>
    /// Determines the way the word will be displayed on a prompt
    /// </summary>
    public DisplayType displayType;
    public CharacterType Type; 
    public char character = ' ';
    public CharacterData data = null;
    public string romaji = null;
    public string meaning = null;

    public bool Check(Character other)
    {
        return character == other.character;
    }

    public string GetDisplaySstring()
    {
        string value = string.Empty;
        switch (displayType)
        {
            case DisplayType.Kanji:
            case DisplayType.Hiragana:
            case DisplayType.Katana:
                value = character.ToString();
                break;
            case DisplayType.Romaji:
                value = WanaKanaSharp.WanaKana.ToRomaji(character.ToString());
                break;
            case DisplayType.Meaning:
                value = meaning;
                break;
        }
        return value;
    }
}

[System.Serializable]
public class Word
{

    /// <summary>
    /// Classification of the word
    /// </summary>
    public CharacterType type;

    public string kanji = null;
    public string[] meanings = null;
    public string romaji = null;
    public string hiragana = null;
    public string katakana = null;

    /// <summary>
    /// Type of response required to complete the word
    /// </summary>
    [System.NonSerialized]
    public InputType responseType;

    /// <summary>
    /// Determines the way the word will be displayed on a prompt
    /// </summary>
    [System.NonSerialized]
    public DisplayType displayType;

    /// <summary>
    /// These are iterated through by the input to complete a word. Not used for display.
    /// They are populated at runtime by the KanjiMananger as the contents are only
    /// known at that time.
    /// </summary>
    [System.NonSerialized]
    public Character[] chars;

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

    public string GetFullKanaString()
    {
        string s = string.Empty;
        for (int i = 0; i < katakana.Length; i++)
        {
            s += katakana[i].ToString();
        }
        return s;
    }

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

    public Character GetChar()
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
public class Sentence
{
    public List<Word> words = new List<Word>();
    private int pIdx { get; set; } = 0;
    public Word currWord { get { return words[pIdx]; } }

    public bool MoveNext()
    {
        if (pIdx + 1 < words.Count) pIdx++;
        return Completed();
    }

    public bool Completed()
    {
        return pIdx == (words.Count - 1);
    }

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
    public List<Sentence> sentences;
}

/// <summary>
/// Used to request prompts from the database
/// </summary>
public enum RequestType
{
    SingleKana, // Prompt that contains one kana
    SingleKanji,// Prompt that contains one kanji
    SingleWord, // Prompt that contains one word
    Sentence, // The prompt contains a list of words that form comprehensible sentence
    Mixed, // A list of random mixed stuff
}

[System.Serializable]
public class PromptConfiguration
{
    public RequestType promptType;

    [HideInInspector]
    public int wordCount;

    public InputType responseType;

    public DisplayType displayType;

    [HideInInspector]
    public string word;

    [HideInInspector]
    public bool useSpecificWord = false;
}

}