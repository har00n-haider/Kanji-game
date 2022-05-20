using Manabu.Core;
using Manabu.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawableTestManager : MonoBehaviour
{

    private Database KanjiDatabase;

    public DrawableCharacter3D kanji3D;

    public DrawableCharacter2D kanji2D;

    public TextAsset kanjiDatabaseFile;

    // Start is called before the first frame update
    void Start()
    {
        KanjiDatabase = new Database();

        KanjiDatabase.Load(kanjiDatabaseFile);

        if(kanji3D.isActiveAndEnabled) kanji3D.Init(KanjiDatabase.GetKanji('人'));
        if(kanji2D.isActiveAndEnabled) kanji2D.Init(KanjiDatabase.GetKanji('人'));


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
