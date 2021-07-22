using System;
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
        refPoints = Utils.GenRefPntsForPnts(rawStroke.points);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach(Vector2 pnt in refPoints) 
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(kanji.gameObject.transform.
                TransformPoint(new Vector3(pnt.x, pnt.y)), 0.1f);
        }
    }
#endif

}

