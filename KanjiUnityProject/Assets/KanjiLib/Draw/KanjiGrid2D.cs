using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using KanjiLib.Core;

namespace KanjiLib.Draw
{

public class KanjiGrid2D : MonoBehaviour
{
    public UILineRenderer gridLinePrefab;
    private float thickness;
    private RectTransform kanjiRectT;
    private RectTransform gridRectT;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(ParsedKanjiData parsedKanji, BoxCollider2D box, float thickness) 
    {
        kanjiRectT = GetComponentInParent<RectTransform>();
        gridRectT = GetComponent<RectTransform>();

        UIUtils.StretchToParentSize(gridRectT, kanjiRectT);
        this.thickness = thickness;
        GenerateGrid(parsedKanji, box.size);
    }


    void GenerateGrid(ParsedKanjiData parsedKanji, Vector2 size)
    {
        Vector2[] gridPnts = new Vector2[]{
            new Vector3( 0.0f, 0.0f), // 0 - bottom left
            new Vector3( 0.5f, 0.0f), // 1 - bottom  middle
            new Vector3( 1.0f, 0.0f), // 2 - bottom right
            new Vector3( 0.0f, 0.5f), // 3 - middle left
            new Vector3( 0.5f, 0.5f), // 4 - middle middle
            new Vector3( 1.0f, 0.5f), // 5 - middle right
            new Vector3( 0.0f, 1.0f), // 6 - top left
            new Vector3( 0.5f, 1.0f), // 7 - top middle
            new Vector3( 1.0f, 1.0f), // 8 - top right
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

    void SetupLineRenderer(UILineRenderer line, Vector2[] pnts, int sIdx, int eIdx) 
    {
        UIUtils.StretchToParentSize(line.rectTransform, gridRectT, Vector2.zero);
        line.Points = new Vector2[] { pnts[sIdx], pnts[eIdx] };
        line.LineThickness = thickness;
    }
}

}