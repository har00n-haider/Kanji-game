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
    private Dictionary<string, CharacterData> kanjis = new Dictionary<string, CharacterData>(); // hex code string to kanji data map
    private PromptList prompts = new PromptList();
    public bool kanjiDataBaseLoaded = false;
    private List<string> meaningsFillerList = new List<string>();

    #region prompt methods
    
    public CharacterData GetRandomKanji()
    {
        if (!kanjiDataBaseLoaded) return null;
        var kanjiList = kanjis.Values.ToList();
        var idx = Random.Range(0, kanjiList.Count - 1);
        return kanjiList[idx];
    }

    // Only hiragana for now...
    public Character GetRandomCharacter(Character except = null, CharacterType type = CharacterType.hiragana) 
    {
        Character prompt = new Character();
        System.Random r = new System.Random();
        List<char> list = new List<char>(unmodifiedHiragana);
        if (except != null) list.Remove(except.character);
        int randomIdx =  r.Next(0, list.Count);
        prompt.Type = CharacterType.hiragana;
        prompt.character = list[randomIdx];
        prompt.romaji = WanaKanaSharp.WanaKana.ToRomaji(prompt.character.ToString());
        return prompt;
    }

    private Sentence GetRandomPromptSentence()
    {
        Sentence prompt = GetRandomPromptSentence();
        foreach (var word in prompt.words)
        {
            GetRandomTestSetForWordType(
                word.type,
                out DisplayType displayType,
                out InputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    private void GetRandomTestSetForWordType(
        CharacterType promptType,
        out DisplayType displayType,
        out InputType responseType)
    {
        displayType = DisplayType.Kanji;
        responseType = InputType.KeyHiragana;

        switch (promptType)
        {
            case CharacterType.kanji:
                displayType = Utils.kanjiPrompts.GetRandomPrompt();
                responseType = Utils.kanjiInputs.GetRandomInput();
                break;

            case CharacterType.hiragana:
                displayType = Utils.hiraganaPrompts.GetRandomPrompt();
                responseType = Utils.hiraganaInputs.GetRandomInput();
                break;

            case CharacterType.katakana:
                displayType = Utils.katakanaPrompts.GetRandomPrompt();
                responseType = Utils.katakanaInputs.GetRandomInput();
                break;

            default:
            break;
        }
    }

    /// <param name="prompt">Prompt that has been configured for a test</param>
    private void SetCharsForPrompt(ref Sentence prompt)
    {
        Action<List<Character>, string> populateCharList =
        (List<Character> cl, string s) =>
        {
            foreach (char c in s)
            {
                cl.Add(new Character()
                {
                    character = c,
                    data = GetKanji(c)
                });
            }
        };

        // Set the chars to iterate through depending
        // on the type of the word and the input type
        foreach (Word word in prompt.words)
        {
            List<Character> chars = new List<Character>();
            switch (word.type)
            {
                case CharacterType.kanji:
                    // take the input type into consideration
                    // for kanji as it could go multpile ways
                    switch (word.responseType)
                    {
                        case InputType.KeyHiraganaWithRomaji:
                        case InputType.KeyHiragana:
                        case InputType.WritingHiragana:
                            populateCharList(chars, word.hiragana);
                            break;

                        case InputType.WritingKanji:
                        case InputType.Meaning:
                            populateCharList(chars, word.kanji);
                            break;
                    }
                    break;
                // hiragana/katana will always only have their own char type
                case CharacterType.hiragana:
                    populateCharList(chars, word.hiragana);
                    break;

                case CharacterType.katakana:
                    populateCharList(chars, word.katakana);
                    break;
            }
            word.chars = chars.ToArray();
        }
    }

    public Sentence GetPromptById(int id)
    {
        if (prompts == null || prompts.sentences.Count == 0) return null;
        Sentence prompt = prompts.sentences[id];
        return prompt;
    }

    // The responsibility of this function is to return
    // a prompt that matches the prompt type
    // TODO: Need to specify exactly what state a prompt is in
    // before returning it
    public Sentence GetPrompt(PromptConfiguration promptConfig)
    {
        Sentence prompt = new Sentence();
        switch (promptConfig.promptType)
        {
            case RequestType.SingleKana:
                // get a random kanji from the kanji list
                char selectedKana = unmodifiedHiragana.ToList().PickRandom();
                prompt.words.Add(new Word()
                {
                    type = CharacterType.hiragana,
                    hiragana = selectedKana.ToString(),
                });
                break;

            case RequestType.SingleKanji:
                // get a random kanji from the kanji list
                CharacterData selectedKanji = kanjis.Values.Where(k => k.category == "required kanji").ToList().PickRandom();
                prompt.words.Add(new Word()
                {
                    type = CharacterType.kanji,
                    kanji = selectedKanji.literal,
                    meanings = selectedKanji.meanings.ToArray(),
                });
                break;

            case RequestType.SingleWord:
                if (promptConfig.useSpecificWord)
                {
                    prompt = prompts.sentences.Where(p => p.words.Count == 1).First(w => w.words[0].hiragana == promptConfig.word);
                }
                else
                {
                    prompt = prompts.sentences.Where(p => p.words.Count == 1).ToList().PickRandom();
                }
                break;

            case RequestType.Sentence:
            case RequestType.Mixed:
            default:
                break;
        }

        // set chars
        foreach (var word in prompt.words)
        {
            word.responseType = promptConfig.responseType;
            word.displayType = promptConfig.displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    #endregion prompt methods

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

    public CharacterData GetRandomKanjiFiltered(System.Func<CharacterData, bool> filter)
    {
        if (!kanjiDataBaseLoaded) return null;
        var kanjiList = kanjis.Values;
        var remainingkanjis = kanjiList.Where(filter).ToList();
        if (remainingkanjis.Count > 0)
        {
            var idx = UnityEngine.Random.Range(0, remainingkanjis.Count - 1);
            return remainingkanjis[idx];
        }
        return null;
    }

    public int CountKanji(System.Func<CharacterData, bool> filter)
    {
        if (!kanjiDataBaseLoaded) return 0;
        var kanjiList = kanjis.Values;
        return kanjiList.Where(filter).Count();
    }

    public CharacterData GetKanji(char kanji)
    {
        CharacterData result = kanjis.Values.FirstOrDefault(k => k.literal == kanji.ToString());
        return result;
    }

    public void Load(TextAsset kanjiDataBaseFile, TextAsset sentenceDataBaseFile = null)
    {
        kanjis = LoadKanjiDatabase(kanjiDataBaseFile).ToDictionary(x => x.code, c => c);
        if(sentenceDataBaseFile != null) 
        {
            prompts = LoadSentenceDatabase(sentenceDataBaseFile);
            meaningsFillerList = LoadMeaningsFillerList();
        }
    }

    private List<CharacterData> LoadKanjiDatabase(TextAsset dataBaseFile)
    {
        List<CharacterData> kanjis = new List<CharacterData>();
        string dbPath = dataBaseFile.text;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dbPath);
        var kanjiElems = xmlDoc.GetElementsByTagName("kanji");
        foreach (XmlNode kanjiElem in kanjiElems)
        {
            CharacterData kanji = new CharacterData();
            kanji.literal = kanjiElem["literal"].InnerText;
            kanji.code = kanjiElem.Attributes["code"].InnerText;
            if (kanjiElem["meaning_group"] != null)
            {
                foreach (XmlNode meaningNode in kanjiElem["meaning_group"])
                {
                    kanji.meanings.Add(meaningNode.InnerText);
                }
            }
            if (kanjiElem["reading_group"] != null)
            {
                foreach (XmlNode readingNode in kanjiElem["reading_group"])
                {
                    if (readingNode.Attributes["r_type"].InnerText == "ja_kun")
                    {
                        kanji.readingsKun.Add(readingNode.InnerText);
                    }
                    else if (readingNode.Attributes["r_type"].InnerText == "ja_on")
                    {
                        kanji.readingsOn.Add(readingNode.InnerText);
                    }
                }
            }
            kanji.svgContent = kanjiElem["svg"].InnerXml;
            kanji.category = kanjiElem["category"].InnerText;
            kanji.categoryType = kanjiElem["category"].Attributes["type"].InnerText;

            kanjis.Add(kanji);
        }
        kanjiDataBaseLoaded = true;
        return kanjis;
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

    public static char[] unmodifiedHiragana = new char[]
    {
        'あ',
        'い',
        'う',
        'え',
        'お',
        'か',
        'き',
        'く',
        'け',
        'こ',
        'さ',
        'し',
        'す',
        'せ',
        'そ',
        'た',
        'ち',
        'つ',
        'て',
        'と',
        'な',
        'に',
        'ぬ',
        'ね',
        'の',
        'は',
        'ひ',
        'ふ',
        'へ',
        'ほ',
        'ま',
        'み',
        'む',
        'め',
        'も',
        'や',
        'ゆ',
        'よ',
        'ら',
        'り',
        'る',
        'れ',
        'ろ',
        'わ',
        'を',
        'ん',
    };





        
    private int pIdx = -1;

    // TODO: debug function
    private Sentence GetNextPrompt()
    {
        ++pIdx;
        var prompt = GetPromptById(pIdx);
        foreach (var word in prompt.words)
        {
            GetTestSetForWordType(
                word.type,
                out DisplayType displayType,
                out InputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    // TODO: debug function, manually written based on what you want to test
    private void GetTestSetForWordType(
    CharacterType promptType,
        out DisplayType displayType,
        out InputType responseType)
    {
        displayType = DisplayType.Kanji;
        responseType = InputType.KeyHiragana;

        switch (promptType)
        {
            case CharacterType.kanji:
                displayType = DisplayType.Kanji;
                responseType = InputType.Meaning;
                break;

            case CharacterType.hiragana:
                displayType = DisplayType.Hiragana;
                responseType = InputType.KeyHiraganaWithRomaji;
                break;

            case CharacterType.katakana:
                displayType = DisplayType.Katana;
                responseType = InputType.KeyKatakanaWithRomaji;
                break;

            default:
                break;
        }
    }


}

}