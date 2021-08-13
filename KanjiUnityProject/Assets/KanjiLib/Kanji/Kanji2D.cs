using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Kanji2D : Kanji
{
    private KanjiGrid2D kanjiGrid;

    public override void Init(KanjiData kanjiData, float scale = 1)
    {
        base.Init(kanjiData, scale);
        // setup the grid
        kanjiGrid = GetComponentInChildren<KanjiGrid2D>();
        kanjiGrid.Init(parsedKanjiData);
    }

    protected override void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            // screen to kanji rect transformation
            Vector2 newPoint = transform.InverseTransformPoint(Input.mousePosition);
            curStroke.inpStroke.AddPoint(newPoint);
        }
        // clear line
        if (Input.GetMouseButtonUp(0))
        {
            curStroke.inpStroke.Complete();
        }
    }

}
