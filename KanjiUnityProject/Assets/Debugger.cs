using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debugger : MonoBehaviour
{

    public KanjiDatabase kanjiManager;

    public Kanji3D kanjiPrefab;
    private Kanji3D currentKanji;

    // Start is called before the first frame update
    void Start()
    {
        GetNewKanji();
    }

    private void GetNewKanji() 
    {
        //KanjiData kanji = kanjiManager.GetKanji('七');
        //kanjiManager.UpdateInputKanji(kanji);
    }

    // Update is called once per frame
    void Update()
    {

        //if (kanjiManager.inputKanji.completed) 
        //{
        //    GetNewKanji();   
        //}
        
    }
}
