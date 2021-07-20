using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KanjiCompletedEventArgs
{
    public KanjiCompletedEventArgs(string text) { Text = text; }
    public string Text { get; } // readonly
}

// Declare the delegate (if using non-generic pattern).
public delegate void KanjiCompletedEventHandler(object sender, KanjiCompletedEventArgs e);

/// <summary>
/// Hosts all the input and reference strokes of the kanji.
/// It compares them, and fires an event when its completed.
/// </summary>
public class Kanji : MonoBehaviour
{
    private List<InputStroke> cmpStrokes = new List<InputStroke>();
    private List<ReferenceStroke> refStrokes = new List<ReferenceStroke>();
    private List<InputStroke> inpStrokes = new List<InputStroke>();
    private ReferenceStroke curRefStroke;
    private InputStroke curInpStroke;


    public ReferenceStroke refStrokePrefab;
    public InputStroke inputStrokePrefab;

    public event KanjiCompletedEventHandler completedEvent;


    private Plane kanjiPlane;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(string path)
    {

        // create an input stroke 
        curInpStroke = Instantiate(inputStrokePrefab, transform).GetComponent<InputStroke>();
        curInpStroke.gameObject.name = "Input stroke " + 1;

        // pull a kanji
        var rawStrokes = KanjiSVGParser.GetStrokesFromSvg(path);
        foreach (RawStroke rawStroke in rawStrokes)
        {

            var stroke = Instantiate(refStrokePrefab, transform).GetComponent<ReferenceStroke>();
            stroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
            stroke.rawStroke = rawStroke;
            stroke.Init();
        }

    }
}
