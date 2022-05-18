using KanjiLib.Core;
using KanjiLib.Draw;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KanjiTest : MonoBehaviour
{

    private KanjiDatabase KanjiDatabase;

    public Kanji3D kanji3D;

    public Kanji2D kanji2D;

    public TextAsset kanjiDatabaseFile;

    // Start is called before the first frame update
    void Start()
    {
        KanjiDatabase = new KanjiDatabase();

        KanjiDatabase.Load(kanjiDatabaseFile);

        if(kanji3D.isActiveAndEnabled) kanji3D.Init(KanjiDatabase.GetKanji('人'));
        if(kanji2D.isActiveAndEnabled) kanji2D.Init(KanjiDatabase.GetKanji('人'));


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
