﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class KanjiGrid2D : MonoBehaviour
{
    public UILineRenderer gridLinePrefab;
    public float thickness;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(ParsedKanjiData parsedKanji) 
    {
        GenerateGrid(parsedKanji);
    }


    void GenerateGrid(ParsedKanjiData parsedKanji)
    {
        Vector2[] gridPnts = new Vector2[]{
        new Vector2( 0.0f, 0.0f), new Vector2( 0.5f, 0.0f), new Vector2( 1.0f, 0.0f),
        new Vector2( 0.0f,-0.5f), new Vector2( 0.5f,-0.5f), new Vector2( 1.0f,-0.5f),
        new Vector2( 0.0f,-1.0f), new Vector2( 0.5f,-1.0f), new Vector2( 1.0f,-1.0f),
        };

        for (int i = 0; i < gridPnts.Length; i++)
        {
            // assuming width/height are the same
            gridPnts[i] *=  parsedKanji.scale*parsedKanji.height;
        }

        // horizontal lines
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 0, 2);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 3, 5);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 6, 8);

        // vertical lines
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 0, 6);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 1, 7);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 2, 8);

    }

    void SetupLineRenderer(UILineRenderer line, Vector2[] pnts, int sIdx, int eIdx) 
    {
        line.Points = new Vector2[] { pnts[sIdx], pnts[eIdx] };
        line.LineThickness = thickness;
    }
}
