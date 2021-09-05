﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;

// Point of access to all the kanji that will be used
// in the game. Should be the only thing that deals with 
// pure data (i.e. no game unity/gameobject stuff) relating
// to kanji
public class KanjiDatabase
{
    private Dictionary<string, KanjiData> kanjis = new Dictionary<string, KanjiData>();
    private Prompts sentences = new Prompts();
    public bool dataBaseLoaded = false;

    public KanjiData GetRandomKanji()
    {
        if (!dataBaseLoaded) return null;
        var kanjiList = kanjis.Values.ToList();
        var idx = Random.Range(0, kanjiList.Count - 1);
        return kanjiList[idx];
    }

    public KanjiData GetRandomKanjiFiltered(System.Func<KanjiData, bool> filter) 
    {
        if (!dataBaseLoaded) return null;
        var kanjiList = kanjis.Values;
        var remainingkanjis = kanjiList.Where(filter).ToList();
        if(remainingkanjis.Count > 0) 
        {
            var idx = Random.Range(0, remainingkanjis.Count - 1);
            return remainingkanjis[idx];
        }
        return null;
    }

    public int CountKanji(System.Func<KanjiData, bool> filter) 
    {
        if (!dataBaseLoaded) return 0;
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
        sentences = LoadSentenceDatabase(sentenceDataBaseFile);
    }

    private List<KanjiData> LoadKanjiDatabase(TextAsset dataBaseFile) 
    {
        List<KanjiData> kanjis = new List<KanjiData>();
        string dbPath = dataBaseFile.text;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dbPath);
        var kanjiElems = xmlDoc.GetElementsByTagName("kanji");
        foreach ( XmlNode kanjiElem in kanjiElems) 
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
                    if(readingNode.Attributes["r_type"].InnerText == "ja_kun") 
                    {
                        kanji.readingsKun.Add(readingNode.InnerText);
                    }
                    else if(readingNode.Attributes["r_type"].InnerText == "ja_on") 
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
        dataBaseLoaded = true;
        return kanjis;
    }

    private Prompts LoadSentenceDatabase(TextAsset dataBaseFile)     
    {
        return JsonUtility.FromJson<Prompts>(dataBaseFile.text);
    }
}

