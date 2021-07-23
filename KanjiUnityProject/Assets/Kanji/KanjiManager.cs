using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;

public class KanjiManager : MonoBehaviour
{
   
    public Kanji kanjiPrefab;

    private Kanji currKanji;
    private List<KanjiData> kanjis = new List<KanjiData>();
    private int kanjiIdx = 0;
    private const string databaseFilename = "kanjigamedb.xml";

    private IKanjiHolder selectedKanjiHolder = null;

    // Start is called before the first frame update
    void Start()
    {
        kanjis = LoadDatabase();
    }

    // Update is called once per frame
    void Update()
    {
        // see if user selected a kanji holder object
        if (Input.GetMouseButtonUp(0)) 
        {
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo)) 
            {
                var kanjiHolder = hitInfo.collider.gameObject.GetComponent<IKanjiHolder>();
                if (kanjiHolder != null)
                {
                    selectedKanjiHolder = kanjiHolder;
                    UpdateKanji(selectedKanjiHolder.kanji);
                }
            }
        }

        if (currKanji != null && currKanji.completed)
        {
            selectedKanjiHolder.Destroy();
            Destroy(currKanji.gameObject);
        }
    }

    public KanjiData GetKanjiData() 
    {
        var remainingkanjis = kanjis.Where(k => k.stats.seen != true).ToList();
        if(remainingkanjis.Count > 0) 
        {
            var idx = Random.Range(0, remainingkanjis.Count - 1);
            remainingkanjis[idx].stats.seen = true;
            return remainingkanjis[idx];
        }
        return null;
    }

    void UpdateKanji(KanjiData kanjiData) 
    {
        if (currKanji != null) Destroy(currKanji.gameObject);
        var kanji = Instantiate(kanjiPrefab, transform).GetComponent<Kanji>();
        kanji.Init(kanjiData);
        currKanji = kanji;
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
