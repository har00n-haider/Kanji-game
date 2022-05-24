using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Manabu.Core
{

    /// <summary>
    /// What is displayed in the prompt.
    /// Limited by word type
    /// </summary>
    public enum DisplayType
    {
        Default,
        Kanji,
        Romaji,
        Hiragana,
        Katakana,
    }

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

    public enum CharacterType
    {
        none = -1,
        kanji = 1,
        hiragana = 2,
        katakana = 3
    }

    /// <summary>
    /// Holds all data relevant to a given japanese charater. This is 
    /// provided by the KanjiDatabase functions
    /// </summary>
    public class Character
    {
        private DisplayType displayType;
        public DisplayType DisplayType { get { return displayType; } set { if (CanSetDisplayType(value)) displayType = value; } }
        public CharacterType type;
        public char literal = ' ';
        public string code = string.Empty;
        public string romaji = string.Empty;
        public List<string> meanings = new List<string>();
        public List<string> readingsOn = new List<string>();
        public List<string> readingsKun = new List<string>();
        public string categoryType = string.Empty;
        public string category = string.Empty;
        public DrawData drawData = null;

        public bool Check(Character other)
        {
            return literal == other.literal;
        }

        private bool CanSetDisplayType(DisplayType displayType)
        {
            switch (displayType)
            {
                case DisplayType.Kanji:
                    if (type == CharacterType.kanji)
                    {
                        return true;
                    }
                    break;
                case DisplayType.Hiragana:
                case DisplayType.Katakana:
                case DisplayType.Romaji:
                    switch (type)
                    {
                        case CharacterType.hiragana:
                        case CharacterType.katakana:
                            return true;
                    }
                    break;
                case DisplayType.Default:
                    return true;
            }
            return false;
        }

        public string GetDisplayString()
        {
            string value = string.Empty;
            switch (displayType)
            {
                case DisplayType.Kanji:
                    if (type == CharacterType.kanji) value = literal.ToString();
                    break;
                case DisplayType.Hiragana:
                    switch (type)
                    {
                        case CharacterType.hiragana:
                            value = literal.ToString();
                            break;
                        case CharacterType.katakana:
                            value = CharacterConversionUtils.KatakanaToHiragana(literal).ToString();
                            break; ;
                    }
                    break;
                case DisplayType.Katakana:
                    switch (type)
                    {
                        case CharacterType.hiragana:
                            value = CharacterConversionUtils.HiraganaToKatakana(literal).ToString();
                            break;
                        case CharacterType.katakana:
                            value = literal.ToString();
                            break; ;
                    }
                    break;
                case DisplayType.Romaji:
                    switch (type)
                    {
                        case CharacterType.hiragana:
                        case CharacterType.katakana:
                            value = CharacterConversionUtils.KanaToRomaji(literal.ToString());
                            break;
                    }
                    break;
                case DisplayType.Default:
                    value = literal.ToString();
                    break;
            }

            Debug.Log("| type: " + type + "| display type: " + displayType + "| literal:" + literal + "| value: " + value);
            return value;
        }
    }

    [System.Serializable]
    public class Word
    {

        public string kanji = null;
        public string[] meanings = null;
        public string romaji = null;
        public string hiragana = null;
        public string katakana = null;

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

        private static readonly char fillerChar = '☐';

        public override string ToString()
        {
            string s = string.Empty;
            foreach (var x in chars)
            {
                s += x.literal;
            }
            return s;
        }

        public void Reset()
        {
            cIdx = 0;
        }


        #region Meaning tracking

        public string GetMeaning()
        {
            return meanings.Length > 0 ? meanings[0] : "-----";
        }

        public bool CheckMeaning(string input)
        {
            return input == GetMeaning();
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
                s += i < cIdx ? chars[i].literal : fillerChar;
            }
            return s;
        }

        public Character GetChar()
        {
            return chars[cIdx];
        }

        public bool CheckChar(char c)
        {
            if (c == chars[cIdx].literal)
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

        [HideInInspector]
        public string word;

        [HideInInspector]
        public bool useSpecificWord = false;
    }

}