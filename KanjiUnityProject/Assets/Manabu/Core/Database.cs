using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using Manabu.Core;
using System;

using UnityEngine; // only for json loading, otherwise should not be used

using Random = UnityEngine.Random;


namespace Manabu.Core
{     

/// <summary>
/// Point of access to all Japanese character/word data. 
/// </summary>
public class Database
{
    /// <summary>
    /// Unicode hex string to character data map. 
    /// </summary>
    private Dictionary<string, Character> characters = new Dictionary<string, Character>();
    private List<Character> kanji = new List<Character>();
    private List<Character> katakana = new List<Character>();
    private List<Character> hiragana = new List<Character>();

    /// <summary>
    /// List of words and sentences 
    /// </summary>
    private PromptList prompts = new PromptList();
    public bool kanjiDataBaseLoaded = false;
    private List<string> meaningsFillerList = new List<string>();

    /// <summary>
    /// Provides a random character from a loaded database
    /// </summary>
    /// <param name="except"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Character GetRandomCharacter(Character except = null, CharacterType type = CharacterType.hiragana) 
    {
        Character character = new Character();
        if (!kanjiDataBaseLoaded) return character;
        System.Random r = new System.Random(DateTime.Now.Millisecond);
        int idx = 0;
        switch (type)
        {
            case CharacterType.kanji:
                List<Character> filteredKanji = except == null ? kanji: kanji.Where((c) => { return c.literal != except.literal; }).ToList();
                if (filteredKanji.Count > 0)
                {
                    idx = r.Next(0, filteredKanji.Count - 1);
                    character = filteredKanji[idx];
                }
                break;
            case CharacterType.hiragana:
                List<Character> filteredHiragana = except == null ? hiragana : hiragana.Where((c) => { return c.literal != except.literal; }).ToList();
                if (filteredHiragana.Count > 0)
                {
                    idx = r.Next(0, filteredHiragana.Count - 1);
                    character = filteredHiragana[idx];
                }
                break;
            case CharacterType.katakana:
                List<Character> filteredKatakana = except == null ? katakana : katakana.Where((c) => { return c.literal != except.literal; }).ToList();
                if (filteredKatakana.Count > 0)
                {
                    idx = r.Next(0, filteredKatakana.Count - 1);
                    character = filteredKatakana[idx];
                }
                break;
        }
        return character;
    }

    public Sentence GetPromptById(int id)
    {
        if (prompts == null || prompts.sentences.Count == 0) return null;
        Sentence prompt = prompts.sentences[id];
        return prompt;
    }

    public Character GetCharacter(char kanji)
    {
        Character result = characters.Values.FirstOrDefault(k => k.literal.ToString() == kanji.ToString());;
        return result;
    }

    public void Load(TextAsset kanjiDataBaseFile, TextAsset sentenceDataBaseFile = null)
    {
        characters = LoadKanjiDatabase(kanjiDataBaseFile).ToDictionary(x => x.code, c => c);
        if(sentenceDataBaseFile != null) 
        {
            prompts = LoadSentenceDatabase(sentenceDataBaseFile);
            meaningsFillerList = LoadMeaningsFillerList();
        }
    }

    private List<Character> LoadKanjiDatabase(TextAsset dataBaseFile)
    {
        List<Character> characters = new List<Character>();
        string dbPath = dataBaseFile.text;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dbPath);
        var kanjiElems = xmlDoc.GetElementsByTagName("kanji");
        foreach (XmlNode kanjiElem in kanjiElems)
        {
            Character character = new Character();
            character.literal = kanjiElem["literal"].InnerText[0]; // should only ever be 1 char long?
            character.code = kanjiElem.Attributes["code"].InnerText;
            if (kanjiElem["meaning_group"] != null)
            {
                foreach (XmlNode meaningNode in kanjiElem["meaning_group"])
                {
                    character.meanings.Add(meaningNode.InnerText);
                }
            }
            if (kanjiElem["reading_group"] != null)
            {
                foreach (XmlNode readingNode in kanjiElem["reading_group"])
                {
                    if (readingNode.Attributes["r_type"].InnerText == "ja_kun")
                    {
                        character.readingsKun.Add(readingNode.InnerText);
                    }
                    else if (readingNode.Attributes["r_type"].InnerText == "ja_on")
                    {
                        character.readingsOn.Add(readingNode.InnerText);
                    }
                }
            }
            character.drawData = SVGParser.GetStrokesFromSvg(kanjiElem["svg"].InnerXml);
            character.category = kanjiElem["category"].InnerText;
            character.categoryType = kanjiElem["category"].Attributes["type"].InnerText;
            character.type = GetTypeFromString(character.category); // HACK: need to have a int here as input
            character.romaji = WanaKanaSharp.WanaKana.ToRomaji(character.literal.ToString());

            characters.Add(character);
            
            switch (character.type)
            {
                case CharacterType.kanji:
                    kanji.Add(character);
                    break;
                case CharacterType.hiragana:
                    //HACK: need to control what goes in the database, or filtering betterc
                    if(CharacterConversionUtils.UnmodifiedHiragana.Contains(character.literal)) hiragana.Add(character);
                    break;
                case CharacterType.katakana:
                    //if(CharacterConversionUtils.UnmodifiedHiragana.Contains(CharacterConversionUtils.KatakanaToHiragana(character.literal))) katakana.Add(character);
                    katakana.Add(character);
                    break;
            }
        }
        kanjiDataBaseLoaded = true;
        return characters;
    }

    private CharacterType GetTypeFromString(string type)
    {
        CharacterType value = CharacterType.none;
        switch (type)
        {
            case "required kanji":
                value = CharacterType.kanji;
                break;
            case "hiragana set":
                value = CharacterType.hiragana;
                break;
            case "katakana set":
                value = CharacterType.katakana;
                break;
            default:
                break;
        }
        return value;
    }

    private PromptList LoadSentenceDatabase(TextAsset dataBaseFile)
    {
        return JsonUtility.FromJson<PromptList>(dataBaseFile.text);
    }

    private List<string> LoadMeaningsFillerList()
    {
        HashSet<string> fillerMeanings = new HashSet<string>();
        foreach (Sentence p in prompts.sentences)
        {
            foreach (Word w in p.words)
            {
                if (w.meanings != null)
                {
                    foreach (string m in w.meanings)
                    {
                        fillerMeanings.Add(m);
                    }
                }
            }
        }
        return fillerMeanings.ToList();
    }


    public List<string> GetRandomFillerMeanings(int noOfStrings, string except)
    {
        if (noOfStrings > meaningsFillerList.Count) return new List<string>();
        HashSet<string> meanings = new HashSet<string>();
        while (meanings.Count < noOfStrings)
        {
            int ridx = Random.Range(0, meaningsFillerList.Count);
            // check for the except string that should not be included
            if (except != null && meaningsFillerList[ridx] != except)
            {
                meanings.Add(meaningsFillerList[ridx]);
            }
            else
            {
                meanings.Add(meaningsFillerList[ridx]);
            }
        }
        return meanings.ToList();
    }

    public Character GetRandomKanjiFiltered(System.Func<Character, bool> filter)
    {
        if (!kanjiDataBaseLoaded) return null;
        var kanjiList = characters.Values;
        var remainingkanjis = kanjiList.Where(filter).ToList();
        if (remainingkanjis.Count > 0)
        {
            var idx = UnityEngine.Random.Range(0, remainingkanjis.Count - 1);
            return remainingkanjis[idx];
        }
        return null;
    }

    public List<Character> GetListOfMatchingCharacters(System.Func<Character, bool> filter)
    {
        if (!kanjiDataBaseLoaded) return new List<Character>();
        return characters.Values.Where(filter).ToList();
    }

    [System.Serializable]
    public class PromptList
    {
        public List<Sentence> sentences;
    }

}

}