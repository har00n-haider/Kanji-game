using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using KanjiLib.Utils;
using KanjiLib.Prompts;
using System;

using UnityEngine; // only for json loading


using Random = UnityEngine.Random;


namespace KanjiLib.Core
{     

// Point of access to all the kanji that will be used
// in the game. Should be the only thing that deals with
// pure data (i.e. no game unity/gameobject stuff) relating
// to kanji
public class KanjiDatabase
{
    private Dictionary<string, KanjiData> kanjis = new Dictionary<string, KanjiData>(); // hex code string to kanji data map
    private PromptList prompts = new PromptList();
    public bool kanjiDataBaseLoaded = false;
    private List<string> meaningsFillerList = new List<string>();

    #region prompt methods
    
    public KanjiData GetRandomKanji()
    {
        if (!kanjiDataBaseLoaded) return null;
        var kanjiList = kanjis.Values.ToList();
        var idx = Random.Range(0, kanjiList.Count - 1);
        return kanjiList[idx];
    }

    // Only hiragana for now...
    public PromptChar GetRandomPromptChar(PromptChar except = null, SymbolType type = SymbolType.hiragana) 
    {
        PromptChar prompt = new PromptChar();
        System.Random r = new System.Random();
        List<char> list = new List<char>(unmodifiedHiragana);
        if (except != null) list.Remove(except.character);
        int randomIdx =  r.Next(0, list.Count);
        prompt.Type = SymbolType.hiragana;
        prompt.character = list[randomIdx];
        prompt.romaji = WanaKanaSharp.WanaKana.ToRomaji(prompt.character.ToString());
        return prompt;
    }

    private PromptSentence GetRandomPromptSentence()
    {
        PromptSentence prompt = GetRandomPromptSentence();
        foreach (var word in prompt.words)
        {
            GetRandomTestSetForWordType(
                word.type,
                out PromptDisplayType displayType,
                out PromptInputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    private void GetRandomTestSetForWordType(
        SymbolType promptType,
        out PromptDisplayType displayType,
        out PromptInputType responseType)
    {
        displayType = PromptDisplayType.Kanji;
        responseType = PromptInputType.KeyHiragana;

        switch (promptType)
        {
            case SymbolType.kanji:
                displayType = Prompts.Utils.kanjiPrompts.GetRandomPrompt();
                responseType = Prompts.Utils.kanjiInputs.GetRandomInput();
                break;

            case SymbolType.hiragana:
                displayType =  Prompts.Utils.hiraganaPrompts.GetRandomPrompt();
                responseType = Prompts.Utils.hiraganaInputs.GetRandomInput();
                break;

            case SymbolType.katakana:
                displayType =  Prompts.Utils.katakanaPrompts.GetRandomPrompt();
                responseType = Prompts.Utils.katakanaInputs.GetRandomInput();
                break;

            default:
                break;
        }
    }

    /// <param name="prompt">Prompt that has been configured for a test</param>
    private void SetCharsForPrompt(ref PromptSentence prompt)
    {
        Action<List<PromptChar>, string> populateCharList =
        (List<PromptChar> cl, string s) =>
        {
            foreach (char c in s)
            {
                cl.Add(new PromptChar()
                {
                    character = c,
                    data = GetKanji(c)
                });
            }
        };

        // Set the chars to iterate through depending
        // on the type of the word and the input type
        foreach (PromptWord word in prompt.words)
        {
            List<PromptChar> chars = new List<PromptChar>();
            switch (word.type)
            {
                case SymbolType.kanji:
                    // take the input type into consideration
                    // for kanji as it could go multpile ways
                    switch (word.responseType)
                    {
                        case PromptInputType.KeyHiraganaWithRomaji:
                        case PromptInputType.KeyHiragana:
                        case PromptInputType.WritingHiragana:
                            populateCharList(chars, word.hiragana);
                            break;

                        case PromptInputType.WritingKanji:
                        case PromptInputType.Meaning:
                            populateCharList(chars, word.kanji);
                            break;
                    }
                    break;
                // hiragana/katana will always only have their own char type
                case SymbolType.hiragana:
                    populateCharList(chars, word.hiragana);
                    break;

                case SymbolType.katakana:
                    populateCharList(chars, word.katakana);
                    break;
            }
            word.chars = chars.ToArray();
        }
    }

    public PromptSentence GetPromptById(int id)
    {
        if (prompts == null || prompts.sentences.Count == 0) return null;
        PromptSentence prompt = prompts.sentences[id];
        return prompt;
    }

    // The responsibility of this function is to return
    // a prompt that matches the prompt type
    // TODO: Need to specify exactly what state a prompt is in
    // before returning it
    public PromptSentence GetPrompt(PromptConfiguration promptConfig)
    {
        PromptSentence prompt = new PromptSentence();
        switch (promptConfig.promptType)
        {
            case PromptRequestType.SingleKana:
                // get a random kanji from the kanji list
                char selectedKana = unmodifiedHiragana.ToList().PickRandom();
                prompt.words.Add(new PromptWord()
                {
                    type = SymbolType.hiragana,
                    hiragana = selectedKana.ToString(),
                });
                break;

            case PromptRequestType.SingleKanji:
                // get a random kanji from the kanji list
                KanjiData selectedKanji = kanjis.Values.Where(k => k.category == "required kanji").ToList().PickRandom();
                prompt.words.Add(new PromptWord()
                {
                    type = SymbolType.kanji,
                    kanji = selectedKanji.literal,
                    meanings = selectedKanji.meanings.ToArray(),
                });
                break;

            case PromptRequestType.SingleWord:
                if (promptConfig.useSpecificWord)
                {
                    prompt = prompts.sentences.Where(p => p.words.Count == 1).First(w => w.words[0].hiragana == promptConfig.word);
                }
                else
                {
                    prompt = prompts.sentences.Where(p => p.words.Count == 1).ToList().PickRandom();
                }
                break;

            case PromptRequestType.Sentence:
            case PromptRequestType.Mixed:
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

    public KanjiData GetRandomKanjiFiltered(System.Func<KanjiData, bool> filter)
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

    public int CountKanji(System.Func<KanjiData, bool> filter)
    {
        if (!kanjiDataBaseLoaded) return 0;
        var kanjiList = kanjis.Values;
        return kanjiList.Where(filter).Count();
    }

    public KanjiData GetKanji(char kanji)
    {
        KanjiData result = kanjis.Values.FirstOrDefault(k => k.literal == kanji.ToString());
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

    private List<KanjiData> LoadKanjiDatabase(TextAsset dataBaseFile)
    {
        List<KanjiData> kanjis = new List<KanjiData>();
        string dbPath = dataBaseFile.text;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dbPath);
        var kanjiElems = xmlDoc.GetElementsByTagName("kanji");
        foreach (XmlNode kanjiElem in kanjiElems)
        {
            KanjiData kanji = new KanjiData();
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
        foreach (PromptSentence p in prompts.sentences)
        {
            foreach (PromptWord w in p.words)
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
    private PromptSentence GetNextPrompt()
    {
        ++pIdx;
        var prompt = GetPromptById(pIdx);
        foreach (var word in prompt.words)
        {
            GetTestSetForWordType(
                word.type,
                out PromptDisplayType displayType,
                out PromptInputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    // TODO: debug function, manually written based on what you want to test
    private void GetTestSetForWordType(
    SymbolType promptType,
        out PromptDisplayType displayType,
        out PromptInputType responseType)
    {
        displayType = PromptDisplayType.Kanji;
        responseType = PromptInputType.KeyHiragana;

        switch (promptType)
        {
            case SymbolType.kanji:
                displayType = PromptDisplayType.Kanji;
                responseType = PromptInputType.Meaning;
                break;

            case SymbolType.hiragana:
                displayType = PromptDisplayType.Hiragana;
                responseType = PromptInputType.KeyHiraganaWithRomaji;
                break;

            case SymbolType.katakana:
                displayType = PromptDisplayType.Katana;
                responseType = PromptInputType.KeyKatakanaWithRomaji;
                break;

            default:
                break;
        }
    }


}

}