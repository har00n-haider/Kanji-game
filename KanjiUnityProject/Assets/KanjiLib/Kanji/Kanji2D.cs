using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Kanji2D : Kanji
{
    private KanjiGrid2D kanjiGrid;

    public override void Init(KanjiData kanjiData)
    {
        base.Init(kanjiData);
        // setup the grid
        kanjiGrid = GetComponentInChildren<KanjiGrid2D>();
        kanjiGrid.Init(parsedKanjiData);
    }

    protected override void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            curStroke.inpStroke.AddPoint(Input.mousePosition);
        }
        // clear line
        if (Input.GetMouseButtonUp(0))
        {
            curStroke.inpStroke.Complete();
        }
    }

}
