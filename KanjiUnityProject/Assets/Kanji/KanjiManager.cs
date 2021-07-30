using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Linq;

public class KanjiManager : MonoBehaviour
{
    // settings for scoring behaviour
    public static readonly int hideReferenceThreshold = 3;

    public TextAsset dataBaseFile;

    private Dictionary<string, KanjiData> kanjis = new Dictionary<string, KanjiData>();
    public bool dataBaseLoaded = false;

    private void Awake()
    {
        kanjis = LoadDatabase().ToDictionary(x => x.code, c => c);

        kanjis = kanjis.Take(1).ToDictionary(i => i.Key, i => i.Value);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        UpdateKanjiSelection();
    }

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

    #region Kanji selection in the world
    // TODO: split out this from the database stuff

    public Kanji inputKanji;
    private IKanjiHolder selectedKanjiHolder = null;
    public Kanji kanjiPrefab;

    void UpdateKanjiSelection() 
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
                    UpdateInputKanji(selectedKanjiHolder.kanji);
                }
            }
        }

        if (inputKanji != null && inputKanji.completed)
        {
            kanjis[inputKanji.kanjiData.code].progress.clears++;
            selectedKanjiHolder.Destroy();
            Destroy(inputKanji.gameObject);
        }

        if (selectedKanjiHolder != null && selectedKanjiHolder.IsDestroyed())
        {
            if (inputKanji != null) Destroy(inputKanji.gameObject);
        }
    }

    public void UpdateInputKanji(KanjiData kanjiData) 
    {
        if (inputKanji != null) Destroy(inputKanji.gameObject);
        var kanji = Instantiate(kanjiPrefab, transform).GetComponent<Kanji>();
        kanji.Init(kanjiData);
        inputKanji = kanji;
    }

    #endregion

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
        dataBaseLoaded = true;
        return kanjis;
    }
}
