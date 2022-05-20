﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Manabu.Core;

namespace Manabu.Examples
{

public class DrawableCharacter2D : DrawableCharacter
{
    // box resize stuff
    // used to scale the normalised kanji points to the dims of the box
    [HideInInspector]
    public BoxCollider2D boxCollider = null;
    private RectTransform rectTransform;

    // grid
    private Grid2D kanjiGrid = null;
    [SerializeField]
    private float gridThickness;


    [HideInInspector]
    private Character currCharTarget = null;

    // refs 
    private Camera mainCamera;

    public override void Init(Character characterData)
    {
        // setup references
        mainCamera = Camera.main;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null && rectTransform != null) PositionCollider();
        
        // setup the grid
        if (kanjiGrid == null)
        {
            kanjiGrid = GetComponentInChildren<Grid2D>();
            kanjiGrid.Init(characterData.drawData, boxCollider, gridThickness);
        }

        // initialise the base class after set up (need the collider configured)
        base.Init(characterData);
    }

    protected override void UpdateInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
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

    public void SetPromptChar(Character promptChar) 
    {
        currCharTarget = promptChar;
        Reset();
        Init(currCharTarget);
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
            Init(currCharTarget);
        }
    }

}

}