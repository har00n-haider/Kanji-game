using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Kanji2D : Kanji
{
    // box resize stuff
    // used to scale the normalised kanji points to the the dims of the box
    [HideInInspector]
    public BoxCollider2D boxCollider = null;
    private RectTransform rectTransform;

    // grid
    private KanjiGrid2D kanjiGrid = null;
    [SerializeField]
    private float gridThickness;

    public override void Init(KanjiData kanjiData)
    {
        if(rectTransform == null) 
        {
            rectTransform = GetComponent<RectTransform>();
        }
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
        if(boxCollider != null && rectTransform != null) 
        {
            ResizeRect();
        }
        // setup the grid
        if (kanjiGrid == null)
        {
            kanjiGrid = GetComponentInChildren<KanjiGrid2D>();
            kanjiGrid.Init(parsedKanjiData, boxCollider, gridThickness);
        }
        // set up before initialising the base class (need the collider set up)
        base.Init(kanjiData);
    }

    protected override void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            // screen to kanji rect transformation
            Vector2 newPoint = transform.InverseTransformPoint(Input.mousePosition);
            curStroke.inpStroke.AddPoint(newPoint);
        }
        // clear line
        if (Input.GetMouseButtonUp(0))
        {
            curStroke.inpStroke.Complete();
        }
    }

    private void ResizeRect() 
    {
        Vector2 size = new Vector2(rectTransform.rect.width, rectTransform.rect.width);
        Vector2 halfSize = size / 2;
        boxCollider.size = size;
        //boxCollider.offset = halfSize;
    }

}
