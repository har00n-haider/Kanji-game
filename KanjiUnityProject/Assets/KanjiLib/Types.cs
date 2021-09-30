using System;
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

    void AddHealth(int health);

    void TakeDamage(int damage);

    Transform getTransform { get; }

    bool isDestroyed { get; }

    PromptConfiguration getPromptConfig { get; }

    System.Action onDestroy { get; set; }

    void OnCurrentPromptSet(Prompt prompt);

    Bounds getBounds();
}

/// <summary>
/// What is displayed in the prompt.
/// Limited by word type
/// </summary>
public enum PromptDisplayType
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
public enum PromptInputType
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

    public string kanji = null;
    public string[] meanings = null;
    public string romaji = null;
    public string hiragana = null;
    public string katakana = null;

    /// <summary>
    /// Type of response required to complete the word
    /// </summary>
    [System.NonSerialized]
    public PromptInputType responseType;

    /// <summary>
    /// Determines the way the word will be displayed on a prompt
    /// </summary>
    [System.NonSerialized]
    public PromptDisplayType displayType;

    /// <summary>
    /// These are iterated through by the input to complete a word. Not used for display.
    /// They are populated at runtime by the KanjiMananger as the contents are only
    /// known at that time.
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
            case PromptInputType.KeyHiragana:
            case PromptInputType.KeyKatakana:
            case PromptInputType.KeyHiraganaWithRomaji:
            case PromptInputType.KeyKatakanaWithRomaji:
            case PromptInputType.WritingHiragana:
            case PromptInputType.WritingKatakana:
            case PromptInputType.WritingKanji:
                return cIdx == chars.Length;

            case PromptInputType.Meaning:
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
    public List<PromptWord> words = new List<PromptWord>();
    private int pIdx { get; set; } = 0;
    public PromptWord currWord { get { return words[pIdx]; } }

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
    // TODO: rename this to simply prompts?
    public List<Prompt> sentences;
}

/// <summary>
/// Used to request prompts from the database
/// </summary>
public enum PromptRequestType
{
    SingleKana, // Prompt contains one kana
    SingleKanji,// Prompt contains one kanji
    SingleWord, // Prompt contains one word
    Sentence, // The prompt contains a list of words that form comprehensible sentence
    Mixed, // A list of random mixed stuff
}

[System.Serializable]
public class PromptConfiguration
{
    public PromptRequestType promptType;

    [HideInInspector]
    public int wordCount;

    public PromptInputType responseType;

    public PromptDisplayType displayType;

    [HideInInspector]
    public string word;

    [HideInInspector]
    public bool useSpecificWord = false;
}