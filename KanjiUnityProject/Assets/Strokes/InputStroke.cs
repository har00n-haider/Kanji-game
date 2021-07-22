using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InputStroke : Stroke
{
    private List<Vector3> inputPoints = new List<Vector3>();
    private Camera mainCam;
    private float offsetFromRef = 0.1f;

    public override void Init(Kanji kanjiManager)
    {
        base.Init(kanjiManager);
        base.SetupLine(Color.blue);
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
                localPoint.z -= offsetFromRef;
                inputPoints.Add(localPoint);
            }
            UpdateLine();
        }
        // clear line
        if (Input.GetMouseButtonUp(0)) 
        {
            // TODO: should really project on to the plane as that is the reference
            refPoints = Utils.GenRefPntsForPnts(
                inputPoints.ConvertAll(p => new Vector2(p.x, p.y)));
            completed = true;
        }
    }

    private void UpdateLine() 
    {
        line.positionCount = inputPoints.Count;
        line.SetPositions(inputPoints.ToArray());
    }

    public void ClearLine() 
    {
        inputPoints.Clear();
        UpdateLine();
        completed = false;
    }



}

