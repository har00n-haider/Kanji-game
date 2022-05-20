using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;

namespace Manabu.Examples
{

public class Grid3D : MonoBehaviour
{
    public LineRenderer gridLinePrefab;
    float gridThickness = 0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(DrawData parsedKanji, BoxCollider box, float gridThickness)
    {
        this.gridThickness = gridThickness;
        GenerateGrid(parsedKanji, box.size);
    }

    // origin assumed at 0,0,0
    void GenerateGrid(DrawData parsedKanji, Vector3 size)
    {
        Vector3[] gridPnts = new Vector3[]{
            new Vector3( 0.0f, 0.0f, 0.5f), // 0 - bottom left
            new Vector3( 0.5f, 0.0f, 0.5f), // 1 - bottom  middle
            new Vector3( 1.0f, 0.0f, 0.5f), // 2 - bottom right
            new Vector3( 0.0f, 0.5f, 0.5f), // 3 - middle left
            new Vector3( 0.5f, 0.5f, 0.5f), // 4 - middle middle
            new Vector3( 1.0f, 0.5f, 0.5f), // 5 - middle right
            new Vector3( 0.0f, 1.0f, 0.5f), // 6 - top left
            new Vector3( 0.5f, 1.0f, 0.5f), // 7 - top middle
            new Vector3( 1.0f, 1.0f, 0.5f), // 8 - top right
        };

        for (int i = 0; i < gridPnts.Length; i++)
        {
            gridPnts[i].Scale(size);
        }

        // horizontal lines
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 0, 2);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 3, 5);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 6, 8);

        // vertical lines
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 0, 6);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 1, 7);
        SetupLineRenderer(Instantiate(gridLinePrefab, transform), gridPnts, 2, 8);

    }

    void SetupLineRenderer(LineRenderer line, Vector3[] pnts, int sIdx, int eIdx)
    {
        line.positionCount = 2;
        line.SetPositions(new Vector3[] { pnts[sIdx], pnts[eIdx] });
        line.startWidth = gridThickness;
        line.endWidth = gridThickness;
    }
}

}