using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InputStroke : Stroke
{
    public override void Init(Kanji kanjiManager)
    {
        base.Init(kanjiManager);
        base.SetupLine();
        line.useWorldSpace = false;
    }

    public override void Awake() 
    {
        base.Awake();
    }

    public void Update()
    {
        if (completed) return;
    }

    public void Complete()
    {
        // TODO: should really project on to the plane as that is the reference
        length = KanjiUtils.GetLengthForPnts(points);
        refPoints = KanjiUtils.GenRefPntsForPnts(points, kanji.noRefPointsInStroke);
        completed = true;
    }

    public void ClearLine() 
    {
        points.Clear();
        UpdateLinePoints();
        completed = false;
    }



}

