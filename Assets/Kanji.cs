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
    private List<InputStroke> completedStrokes = new List<InputStroke>();
    private List<ReferenceStroke> refStrokes = new List<ReferenceStroke>();
    private List<InputStroke> inputStrokes = new List<InputStroke>();
    private int curRefStrokeIdx;
    private ReferenceStroke referenceStroke;
    private InputStroke curInpStroke;


    public ReferenceStroke refStrokePrefab;
    public InputStroke inputStrokePrefab;

    public event KanjiCompletedEventHandler completedEvent;


    public float comparisonThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Compare();
    }

    public void Init(string path, KanjiManager kanjiManager)
    {
        // create the plane on which the kanji will be drawn
        Vector3 planePoint = kanjiManager.gameObject.transform.position +
            kanjiManager.gameObject.transform.forward * kanjiManager.distanceToKanji;
        Vector3 planeDir = -kanjiManager.gameObject.transform.forward;
        Plane kanjiPlane = new Plane(planeDir.normalized, planePoint);

        // pull a kanji
        var rawStrokes = KanjiSVGParser.GetStrokesFromSvg(path);
        foreach (RawStroke rawStroke in rawStrokes)
        {
            // assuming we get these in order
            var stroke = Instantiate(refStrokePrefab, transform).GetComponent<ReferenceStroke>();
            stroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
            stroke.rawStroke = rawStroke;
            stroke.Init(kanjiPlane, kanjiManager);
            refStrokes.Add(stroke);
            curRefStrokeIdx = 0;
        }

        // create the first input stroke 
        curInpStroke = Instantiate(inputStrokePrefab, transform).GetComponent<InputStroke>();
        curInpStroke.gameObject.name = "Input stroke " + 1;
        curInpStroke.Init(kanjiPlane, kanjiManager);

    }

    private void Compare() 
    {
        
        if (curInpStroke.completed) 
        {
            bool result = true;
            for (int i = 0; i < curInpStroke.refPoints.Count; i++)
            {
                float distance = Mathf.Abs((curInpStroke.refPoints[i] - curRefStroke.refPoints[i]).magnitude);
                result &= distance < comparisonThreshold;
            }
            if (result) 
            {
            
            }
        
        }
    
    }
}
