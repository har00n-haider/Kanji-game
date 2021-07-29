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
        base.SetupLine(Color.grey);
        // use the raw kanji data to create lines in the world
        line.positionCount = rawStroke.points.Count;
        line.SetPositions(rawStroke.points.ConvertAll(p => new Vector3(p.x, p.y)).ToArray());
        line.useWorldSpace = false;
        refPoints = Utils.GenRefPntsForPnts(rawStroke.points, kanji.noRefPointsInStroke);

        highlightData.initialColor = Color.red;
        highlightData.initialWidth = width * 3;

        completed = true;
    }

}

