﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class KanjiCompletedEventArgs
{
    public string Text { get; set; } 
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
    private ReferenceStroke curRefStroke { get { return refStrokes[curRefStrokeIdx]; } }
    private InputStroke curInpStroke;


    public ReferenceStroke refStrokePrefab;
    public InputStroke inputStrokePrefab;

    public event KanjiCompletedEventHandler completedEvent;

    private float comparisonThreshold = 0.5f;

    private Plane kanjiPlane = new Plane();

    private bool completed = false;
        
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(!completed) Compare();
    }

    public void Init(string path)
    {
        // create the plane on which the kanji will be drawn
        Vector3 planePoint = gameObject.transform.position;
        Vector3 planeDir = -gameObject.transform.forward;
        kanjiPlane = new Plane(planeDir.normalized, planePoint);

        // pull a kanji
        var rawStrokes = KanjiSVGParser.GetStrokesFromSvg(path);
        foreach (RawStroke rawStroke in rawStrokes)
        {
            // assuming we get these in order
            var stroke = Instantiate(refStrokePrefab, transform).GetComponent<ReferenceStroke>();
            stroke.gameObject.name = "Reference Stroke " + rawStroke.orderNo;
            stroke.rawStroke = rawStroke;
            stroke.Init(kanjiPlane, this);
            refStrokes.Add(stroke);
            curRefStrokeIdx = 0;
        }

        curInpStroke = GenerateInputStroke();
    }

    private InputStroke GenerateInputStroke() 
    {
        // create the first input stroke 
        var inputStroke = Instantiate(inputStrokePrefab, transform).GetComponent<InputStroke>();
        inputStroke.gameObject.name = "Input stroke " + (curRefStrokeIdx + 1);
        inputStroke.Init(kanjiPlane, this);
        return inputStroke;
    }

    private void Compare() 
    {
        if (curInpStroke.completed) 
        {
            bool isRefStrokeGood = true;
            for (int i = 0; i < curInpStroke.refPoints.Count; i++)
            {
                float distance = Mathf.Abs((
                    curInpStroke.refPoints[i] - 
                    curRefStroke.refPoints[i]).magnitude);
                isRefStrokeGood &= distance < comparisonThreshold;
            }
            if (isRefStrokeGood) 
            {
                Debug.Log("Good!");
                completedStrokes.Add(curInpStroke);
                if(curRefStrokeIdx == (refStrokes.Count - 1))
                {
                    completedEvent?.Invoke(this, new KanjiCompletedEventArgs());
                    return;
                }
                else 
                {
                    curRefStrokeIdx++;
                    curInpStroke = GenerateInputStroke();
                }
            }
            else 
            {
                curInpStroke.ClearLine();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawPlane(kanjiPlane, kanjiPlane.ClosestPointOnPlane(Vector3.zero), new Color(0,0,1,0.1f));    
    }

    private void DrawPlane(Plane p, Vector3 center, Color color, float radius = 10)
    {
        // our plane as a circle mesh
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        Vector3 p0 = p.ClosestPointOnPlane(Vector3.zero);
        Vector3 p1 = p.ClosestPointOnPlane(Camera.main.transform.up);
        // flip normal if its on the wrong side
        if (p.GetDistanceToPoint(Camera.main.transform.position) < 0)
        {
            p.SetNormalAndPosition(p.normal * -1, p0);
        }
        Vector3 planeVec = (p0 - p1).normalized;
        verts.Add(center);
        verts.Add(center + planeVec * radius);
        for (float i = 10; i <= 360; i += 10)
        {
            Quaternion q = Quaternion.AngleAxis(i, p.normal);
            Vector3 circleVec = q * planeVec;
            Vector3 newPnt = center + circleVec * radius;
            verts.Add(newPnt);
            tris.Add(0);
            tris.Add(verts.Count - 2);
            tris.Add(verts.Count - 1);
        }
        Mesh circleMesh = new Mesh
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray()
        };
        circleMesh.RecalculateNormals();
        if (circleMesh.vertexCount > 0)
        {
            Gizmos.color = color;
            Gizmos.DrawMesh(circleMesh);
        }
    }

#endif
}
