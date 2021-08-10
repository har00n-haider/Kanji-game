using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InputStroke : Stroke
{
    private Camera mainCam;
    private float offsetFromRef = 0.1f;

    public override void Init(Kanji kanjiManager)
    {
        base.Init(kanjiManager);
        base.SetupLine();
        line.useWorldSpace = false;
    }

    public override void Awake() 
    {
        base.Awake();

        mainCam = Camera.main;
    }

    public void Update()
    {
        if (completed) return;

        // populate line
        if (Input.GetMouseButton(0)) 
        {
            // convert mouse position to a point on the kanji plane 
            Ray ray =  mainCam.ScreenPointToRay(Input.mousePosition);
            bool hit = kanji.GetPlane().Raycast(ray, out float enter);
            if (hit) 
            {
                Vector3 worldPoint = ray.direction * enter + ray.origin;
                Vector3 localPoint = kanji.transform.InverseTransformPoint(worldPoint);
                points.Add(localPoint);
                UpdateLinePoints();
            }
        }
        // clear line
        if (Input.GetMouseButtonUp(0)) 
        {
            // TODO: should really project on to the plane as that is the reference
            length = KanjiUtils.GetLengthForPnts(points);
            refPoints = KanjiUtils.GenRefPntsForPnts(points, kanji.noRefPointsInStroke);
            completed = true ;
        }
    }

    public void ClearLine() 
    {
        points.Clear();
        UpdateLinePoints();
        completed = false;
    }



}

