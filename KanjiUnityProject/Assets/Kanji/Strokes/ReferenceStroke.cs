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
        base.SetupLine();
        // use the raw kanji data to create lines in the world
        points = rawStroke.points;
        refPoints = Utils.GenRefPntsForPnts(rawStroke.points, kanji.noRefPointsInStroke);
        length = Utils.GetLengthForPnts(points);
        UpdateLinePoints();
        line.useWorldSpace = false;
        completed = true;
    }

}

