using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class KanjiManager : MonoBehaviour
{
    List<string> kanjiPathList = new List<string>();

    public Kanji kanjiPrefab;

    private Kanji currKanji;

    List<KanjiData> kanjis = new List<KanjiData>();
    int kanjiIdx = 0;

    const string databaseFilename = "kanjigamedb.xml";

    // Start is called before the first frame update
    void Start()
    {
        kanjis = LoadDatabase();
        if(kanjis.Count > 0) 
        {
            currKanji = GenKanji(kanjis[kanjiIdx]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currKanji.completed) 
        {
            Destroy(currKanji?.gameObject);
            kanjiIdx++;
            if(kanjiIdx < kanjis.Count) 
            {
                currKanji = GenKanji(kanjis[kanjiIdx]);
            }
        }
    }

    Kanji GenKanji(KanjiData kanjiData) 
    {
        var kanji = Instantiate(kanjiPrefab, transform).GetComponent<Kanji>();
        kanji.Init(kanjiData);
        return kanji;
    }

    List<KanjiData> LoadDatabase() 
    {
        List<KanjiData> kanjis = new List<KanjiData>();
        string dbPath = Path.Combine(Application.dataPath, "Database", databaseFilename);
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(dbPath);
        var kanjiElems = xmlDoc.GetElementsByTagName("kanji");
        foreach ( XmlNode kanjiElem in kanjiElems) 
        {
            KanjiData kanji = new KanjiData();
            kanji.literal = kanjiElem["literal"].InnerText;
            kanji.code = kanjiElem.Attributes["code"].InnerText;
            foreach (XmlNode meaningNode in kanjiElem["meaning_group"]) 
            {
                kanji.meanings.Add(meaningNode.InnerText);
            }
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
            kanji.svgContent = kanjiElem["svg"].InnerXml;
            kanji.category = new System.Tuple<KanjiData.CategoryType, string>
                (KanjiData.CategoryType.kanjipower, kanjiElem["category"].InnerText);
            kanjis.Add(kanji);
        }
        return kanjis;
    }
}
