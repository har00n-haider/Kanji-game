using KanjiLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace KanjiLib.Prompts
{

public enum SymbolType
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
    /// <summary>
    /// Determines the way the word will be displayed on a prompt
    /// </summary>
    public PromptDisplayType displayType;
    public SymbolType Type; 
    public char character = ' ';
    public KanjiData data = null;
    public string romaji = null;
    public string meaning = null;

    public bool Check(PromptChar other)
    {
        return character == other.character;
    }

    public string GetDisplaySstring()
    {
        string value = string.Empty;
        switch (displayType)
        {
            case PromptDisplayType.Kanji:
            case PromptDisplayType.Hiragana:
            case PromptDisplayType.Katana:
                value = character.ToString();
                break;
            case PromptDisplayType.Romaji:
                value = WanaKanaSharp.WanaKana.ToRomaji(character.ToString());
                break;
            case PromptDisplayType.Meaning:
                value = meaning;
                break;
        }
        return value;
    }
}

[System.Serializable]
public class PromptWord
{

    /// <summary>
    /// Classification of the word
    /// </summary>
    public SymbolType type;

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
public class PromptSentence
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
    public List<PromptSentence> sentences;
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

}