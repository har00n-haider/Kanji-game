using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using KanjiLib.Core;
using KanjiLib.Prompts;

namespace KanjiLib.Draw
{

public class Kanji2D : Kanji
{
    // box resize stuff
    // used to scale the normalised kanji points to the dims of the box
    [HideInInspector]
    public BoxCollider2D boxCollider = null;
    private RectTransform rectTransform;

    // grid
    private KanjiGrid2D kanjiGrid = null;
    [SerializeField]
    private float gridThickness;


    [HideInInspector]
    private PromptChar currCharTarget = null;

    // refs 
    private Camera keyboardCamera;

    public override void Init(KanjiData kanjiData)
    {
        // setup references
        keyboardCamera = GameObject.FindGameObjectWithTag("KeyboardCamera").GetComponent<Camera>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null && rectTransform != null) PositionCollider();
        
        // setup the grid
        if (kanjiGrid == null)
        {
            kanjiGrid = GetComponentInChildren<KanjiGrid2D>();
            kanjiGrid.Init(parsedKanjiData, boxCollider, gridThickness);
        }

        // initialise the base class after set up (need the collider configured)
        base.Init(kanjiData);
    }

    protected override void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 worldPos = keyboardCamera.ScreenToWorldPoint(Input.mousePosition);
            // take input only if the mouse is clicked within the box collider
            if (boxCollider.OverlapPoint(worldPos)) 
            {
                // screen to kanji rect transformation
                Vector2 localPos = transform.InverseTransformPoint(worldPos);
                Vector2 normLocPoint = GeometryUtils.NormalizePointToBoxPosOnly(boxCollider.size, localPos);
                curStroke.inpStroke.AddPoint(normLocPoint);
            }
        }
        // clear line
        if (Input.GetMouseButtonUp(0))
        {
            curStroke.inpStroke.Complete();
        }
    }

    // makes sure the collider is in line with the 2d rect 
    private void PositionCollider()
    {
        Vector2 size = new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        Vector2 halfSize = size/2;
        boxCollider.size = size;
        boxCollider.offset = halfSize;
    }

    public void SetPromptChar(PromptChar promptChar) 
    {
        currCharTarget = promptChar;
        Reset();
        Init(currCharTarget.data);
    }

    protected override void Completed() 
    {
        if (pass) 
        {
            //keyboard.CharUpdated(currCharTarget.character);
        }
        else
        {
            Reset();
            Init(currCharTarget.data);
        }
    }

}

}