using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ReferenceStroke : Stroke
{

    public RawStroke rawStroke;

    public override void Init()
    {
        line.positionCount = rawStroke.points.Count;
        line.SetPositions(rawStroke.points.ConvertAll(p => new Vector3(p.x, p.y)).ToArray());
        line.useWorldSpace = false;
        line.startWidth = width;
        line.endWidth = width;
    }

}

