using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Manabu.Core;


namespace Manabu.Examples
{

/// <summary>
/// Example of how to use the character data from from Manabu to create a 
/// drawable character on the screen
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class DrawableCharacter3D : DrawableCharacter
{
    // box resize stuff
    // used to scale the normalised kanji points to the the dims of the box
    [HideInInspector]
    public BoxCollider boxCollider = null;
    // width/height size
    [SerializeField]
    private Vector3 boxColliderSize;
    [SerializeField]
    public float kanjiZBoxRelativepos = 0.5f;

    // grid
    private Grid3D kanjiGrid = null;
    [SerializeField]
    private float gridThickness;

    public override void Init(CharacterData kanjiData)
    {
        // set up before initialising the base class (need the collider set up)
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
            ResizeCollider();
        }
        // setup the grid
        if (kanjiGrid == null)
        {
            kanjiGrid = GetComponentInChildren<Grid3D>();
            kanjiGrid.Init(parsedKanjiData, boxCollider, gridThickness);
        }

        base.Init(kanjiData);
    }

    private void ResizeCollider()
    {
        Vector3 halfSize = boxColliderSize / 2;
        boxCollider.size = boxColliderSize;
        boxCollider.center = halfSize;
    }

    protected override void UpdateInput()
    {
        // populate line
        if (Input.GetMouseButton(0))
        {
            // convert mouse position to a point on the kanji plane 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (boxCollider.Raycast(ray, out RaycastHit hitInfo, 10000)) 
            {
                bool hit = GetPlane().Raycast(ray, out float enter);
                if (hit)
                {
                    // normalize the input points for correct comparison with ref stroke
                    Vector3 worldPoint = ray.direction * enter + ray.origin;
                    Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                    Vector3 normLocPoint = GeometryUtils.NormalizePointToBoxPosOnly(boxCollider.size, localPoint);
                    curStroke.inpStroke.AddPoint(normLocPoint);
                }
            };


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
        if (boxCollider == null) return new Plane();
        // create the plane on which the kanji will be drawn
        Vector3 planePoint = transform.TransformPoint(boxCollider.center);
        Vector3 planeDir = -gameObject.transform.forward;
        return new Plane(planeDir.normalized, planePoint);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // plane
        Plane kanjiPlane = GetPlane();
        DrawPlane(kanjiPlane, kanjiPlane.ClosestPointOnPlane(transform.position), new Color(0, 0, 1, 0.1f));

        // draw debug strokes
        if (strokes.Count > 0)
        {
            for (int i = 0; i <= curStrokeIdx; i++)
            {
                if (strokes[i].strokeResult != null) DrawStrokePair(strokes[i]);
            }
        }
    }

    private void DrawStrokePair(StrokePair sp)
    {
        if (sp.isValid)
        {
            for (int i = 0; i < config.noRefPointsInStroke; i++)
            {
                Gizmos.color = Color.gray;
                var refPnt = transform.TransformPoint(new Vector3(sp.refStroke.refPoints[i].x, sp.refStroke.refPoints[i].y));
                Gizmos.DrawSphere(refPnt, 0.1f);
                Gizmos.color = new Color(0, 0, 0, 0.1f);
                Gizmos.DrawSphere(refPnt, config.compThreshLoose);
                Gizmos.DrawSphere(refPnt, config.compThreshTight);
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

}