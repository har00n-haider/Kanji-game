using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;
using KanjiLib.Utils;

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

    public KanjiData GetRandomKanji()
    {
        if (!kanjiDataBaseLoaded) return null;
        var kanjiList = kanjis.Values.ToList();
        var idx = Random.Range(0, kanjiList.Count - 1);
        return kanjiList[idx];
    }

    #region prompt methods

    public Prompt GetRandomPrompt()
    {
        var idx = Random.Range(0, prompts.sentences.Count - 1);
        return GetPromptById(idx);
    }

    public Prompt GetPromptById(int id)
    {
        if (prompts == null || prompts.sentences.Count == 0) return null;
        Prompt prompt = prompts.sentences[id];
        return prompt;
    }

    // The responsibility of this function is to return
    // a prompt that matches the prompt type
    // TODO: Need to specify exactly what state a prompt is in
    // before returning it
    public Prompt GetPrompt(PromptConfiguration promptConfig)
    {
        Prompt prompt = new Prompt();
        switch (promptConfig.promptType)
        {
            case PromptRequestType.SingleKana:
                // get a random kanji from the kanji list
                char selectedKana = KanjiUtils.unmodifiedHiragana.ToList().PickRandom();
                prompt.words.Add(new PromptWord()
                {
                    type = PromptWord.WordType.hiragana,
                    hiragana = selectedKana.ToString(),
                });
                break;

            case PromptRequestType.SingleKanji:
                // get a random kanji from the kanji list
                KanjiData selectedKanji = kanjis.Values.Where(k => k.category == "required kanji").ToList().PickRandom();
                prompt.words.Add(new PromptWord()
                {
                    type = PromptWord.WordType.kanji,
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
            var idx = Random.Range(0, remainingkanjis.Count - 1);
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

    public void Load(TextAsset kanjiDataBaseFile, TextAsset sentenceDataBaseFile)
    {
        kanjis = LoadKanjiDatabase(kanjiDataBaseFile).ToDictionary(x => x.code, c => c);
        prompts = LoadSentenceDatabase(sentenceDataBaseFile);
        meaningsFillerList = LoadMeaningsFillerList();
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
        foreach (Prompt p in prompts.sentences)
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
}

}