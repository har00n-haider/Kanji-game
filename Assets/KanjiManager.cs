using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class KanjiManager : MonoBehaviour
{
    List<string> kanjiPathList = new List<string>();

    public Camera mainCam;

    public Kanji currKanji;

    public float distanceInfrontOfCam;

    // Start is called before the first frame update
    void Start()
    {
        GenKanji();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenKanji() 
    {
        var kanji = Instantiate(currKanji, transform).GetComponent<Kanji>();
        kanji.gameObject.transform.Translate(new Vector3(0, 0, distanceInfrontOfCam));
        kanji.Init(Path.Combine(Application.dataPath, "09920.svg"));
    }
}
