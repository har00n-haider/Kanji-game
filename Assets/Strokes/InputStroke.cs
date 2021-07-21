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


    public override void Init(Plane kanjiPlane, KanjiManager kanjiManager)
    {
        base.Init(kanjiPlane, kanjiManager);
        base.SetupLine(Color.blue);
        line.useWorldSpace = true;
    }

    public override void Awake() 
    {
        base.Awake();

        mainCam = Camera.main;
    }

    public void Update()
    {
        // populate line
        if (Input.GetMouseButton(0)) 
        {
            // convert mouse position to a point on the kanji plane 
            Ray ray =  mainCam.ScreenPointToRay(Input.mousePosition);
            bool hit = kanjiPlane.Raycast(ray, out float enter);
            if (hit) 
            {
                Vector3 inputPoint = ray.direction * enter + ray.origin;
                inputPoints.Add(inputPoint);
            }
            UpdateLine();
        }
        // clear line
        if (Input.GetMouseButtonUp(0)) 
        {
            refPoints = Utils.GenRefPntsForPnts(refPoints);
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
    }



}

