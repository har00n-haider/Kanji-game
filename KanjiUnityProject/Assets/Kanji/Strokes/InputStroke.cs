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
        strokeRenderer.SetupLine();
    }

    public override void Awake() 
    {
        base.Awake();
    }

    public void Update()
    {
    }

    public void AddPoint(Vector2 point) 
    {
        points.Add(point);
        strokeRenderer.UpdateLinePoints(points);
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
        strokeRenderer.UpdateLinePoints(points);
        completed = false;
    }
}

