using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;

public class KanjiManager : MonoBehaviour
{

    public TextAsset dataBaseFile;
    public Kanji kanjiPrefab;

    private Kanji currKanji;
    private Dictionary<string, KanjiData> kanjis = new Dictionary<string, KanjiData>();

    private IKanjiHolder selectedKanjiHolder = null;

    private int clearRequirement = 1;
    public int kanjiToBeCleared = 0;

    // Start is called before the first frame update
    void Start()
    {
        kanjis = LoadDatabase().ToDictionary( x => x.code, c => c);
    }

    // Update is called once per frame
    void Update()
    {
        // see if user selected a kanji holder object
        if (Input.GetMouseButtonUp(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo)) 
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
            kanjis[currKanji.data.code].stats.timesCleared++;
            selectedKanjiHolder.Destroy();
            Destroy(currKanji.gameObject);
        }

        if(selectedKanjiHolder != null && selectedKanjiHolder.IsDestroyed()) 
        {
            if(currKanji != null) Destroy(currKanji.gameObject);
        }
    }

    public KanjiData GetKanjiData() 
    {
        var kanjiList = kanjis.Values;
        var remainingkanjis = kanjiList.Where(k => k.stats.timesCleared < clearRequirement).ToList();
        kanjiToBeCleared = remainingkanjis.Count;
        Debug.Log(string.Format("Remaining kanji {0}", kanjiToBeCleared));
        if(remainingkanjis.Count > 0) 
        {
            var idx = Random.Range(0, remainingkanjis.Count - 1);
            remainingkanjis[idx].stats.seen = true;
            return remainingkanjis[idx];
        }
        return null;
    }

    public void IncrementClearRequirement() 
    {
        clearRequirement++;
        var kanjiList = kanjis.Values;
        var remainingkanjis = kanjiList.Where(k => k.stats.timesCleared < clearRequirement).ToList();
        kanjiToBeCleared = remainingkanjis.Count;
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
        string dbPath = dataBaseFile.text;

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(dbPath);
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
