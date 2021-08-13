using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Kanji3D : Kanji
{
    private KanjiGrid3D kanjiGrid;

    public override void Init(KanjiData kanjiData)
    {
        base.Init(kanjiData);
        // setup the grid
        kanjiGrid = GetComponentInChildren<KanjiGrid3D>();
        kanjiGrid.Init(parsedKanjiData);
    }

    protected override void UpdateInput()
    {
        //TODO: move the input stuff to Kanji
        // populate line
        if (Input.GetMouseButton(0))
        {
            // convert mouse position to a point on the kanji plane 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool hit = GetPlane().Raycast(ray, out float enter);
            if (hit)
            {
                Vector3 worldPoint = ray.direction * enter + ray.origin;
                Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                curStroke.inpStroke.AddPoint(localPoint);
            }
        }
        // clear line
        if (Input.GetMouseButtonUp(0))
        {
            curStroke.inpStroke.Complete();
        }
    }

    // Get the plane on which the the 3d kanji lies
    private Plane GetPlane()
    {
        // create the plane on which the kanji will be drawn
        Vector3 planePoint = gameObject.transform.position;
        Vector3 planeDir = -gameObject.transform.forward;
        return new Plane(planeDir.normalized, planePoint);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debug) return;
        // plane
        Plane kanjiPlane = GetPlane();
        DrawPlane(kanjiPlane, kanjiPlane.ClosestPointOnPlane(transform.position), new Color(0, 0, 1, 0.1f));

        // draw debug strokes
        if (strokes.Count > 0)
        {
            for (int i = 0; i <= curStrokeIdx; i++)
            {
                if(strokes[i].strokeResult != null) DrawStrokePair(strokes[i]);
            }
        }
    }

    private void DrawStrokePair(StrokePair sp)
    {
        if (sp.isValid)
        {
            for (int i = 0; i < noRefPointsInStroke; i++)
            {
                Gizmos.color = Color.gray;
                var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.refPoints[i].x, sp.refStroke.refPoints[i].y));
                Gizmos.DrawSphere(refPnt, 0.1f);
                Gizmos.color = new Color(0,0,0,0.1f);
                Gizmos.DrawSphere(refPnt, compThreshLoose);
                Gizmos.DrawSphere(refPnt, compThreshTight);
                Gizmos.color = sp.strokeResult.pass ? Color.green : Color.red;
                // tight dist color
                Gizmos.color = sp.strokeResult.tightPointIdx == i ? new Color(1, 0, 1) : Gizmos.color; // purple
                var inpPnt = transform.TransformPoint(new Vector3(sp.inpStroke.refPoints[i].x, sp.inpStroke.refPoints[i].y));
                Gizmos.DrawSphere(inpPnt, 0.1f);
                // connect the two
                Gizmos.color = Color.red;
                Gizmos.DrawLine(refPnt, inpPnt);
            }
        }
    }

    private void DrawPlane(Plane p, Vector3 center, Color color, float radius = 10)
    {
        // our plane as a circle mesh
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        Vector3 p0 = p.ClosestPointOnPlane(Vector3.zero);
        Vector3 p1 = p.ClosestPointOnPlane(Camera.main.transform.up);
        // flip normal if its on the wrong side
        if (p.GetDistanceToPoint(Camera.main.transform.position) < 0)
        {
            p.SetNormalAndPosition(p.normal * -1, p0);
        }
        Vector3 planeVec = (p0 - p1).normalized;
        verts.Add(center);
        verts.Add(center + planeVec * radius);
        for (float i = 10; i <= 360; i += 10)
        {
            Quaternion q = Quaternion.AngleAxis(i, p.normal);
            Vector3 circleVec = q * planeVec;
            Vector3 newPnt = center + circleVec * radius;
            verts.Add(newPnt);
            tris.Add(0);
            tris.Add(verts.Count - 2);
            tris.Add(verts.Count - 1);
        }
        Mesh circleMesh = new Mesh
        {
            vertices = verts.ToArray(),
            triangles = tris.ToArray()
        };
        circleMesh.RecalculateNormals();
        if (circleMesh.vertexCount > 0)
        {
            Gizmos.color = color;
            Gizmos.DrawMesh(circleMesh);
        }
    }

#endif
}
