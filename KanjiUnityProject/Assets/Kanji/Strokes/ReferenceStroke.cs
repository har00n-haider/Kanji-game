using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ReferenceStroke : Stroke
{
    public RawStroke rawStroke;

    public override void Init(Kanji kanji)
    {
        base.Init(kanji);
        strokeRenderer.SetupLine();
        // use the raw kanji data to create lines in the world
        points = rawStroke.points;
        refPoints = KanjiUtils.GenRefPntsForPnts(rawStroke.points, kanji.noRefPointsInStroke);
        length = KanjiUtils.GetLengthForPnts(points);
        strokeRenderer.UpdateLinePoints(points);
        completed = true;
    }
}

