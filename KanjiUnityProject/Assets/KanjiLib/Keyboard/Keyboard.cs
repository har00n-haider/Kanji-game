﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Keyboard : MonoBehaviour
{
    // keyboard 
    [SerializeField]
    private CharType lastType;
    public CharType type { get; set; }

    // refs
    [SerializeField]
    private FlickLayout hiraganaFlickInput;
    [SerializeField]
    private FlickLayout katakanaFlickInput;
    [SerializeField]
    private FlickLayout romajiFlickInput;
    [SerializeField]
    private Kanji2D drawInput;

    private KanjiManager kanjiMan;

    public PromptChar currCharTarget { get; set; } = new PromptChar();

    private void Awake() 
    { 
        kanjiMan = GameObject.FindGameObjectWithTag("KanjiManager").GetComponent<KanjiManager>();
    }

    private void Start()
    {
        // flick layout setup
        hiraganaFlickInput.keyboard = this;
        hiraganaFlickInput.Init();
        katakanaFlickInput.keyboard = this;
        katakanaFlickInput.Init();
        romajiFlickInput.keyboard = this;
        romajiFlickInput.Init();

        // needs KanjiData from the kanjimanager , i.e. the current kanji to draw
        drawInput.keyboard = this;
    }

    private void Update()
    {
        if (type == lastType) return;
        switch (type) 
        {
            case CharType.Draw:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;
            case CharType.Hiragana:
                hiraganaFlickInput.gameObject.SetActive(true);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                break;
            case CharType.Katana:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(true);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                break;
            case CharType.Romaji:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(true);
                drawInput.gameObject.SetActive(false);
                break;
        }
        lastType = type;
    }

    // called from the flicklayouts and the Kanji2D input mechanisms
    public void UpdateCharacter(string character) 
    {
        Debug.Log(character);
        if (character == currCharTarget.character)
        {
            kanjiMan.UpdateCurrentKanjiTraceable(true);
        }
    }

    public void SetPromptChar(PromptChar promptChar) 
    {
        currCharTarget = promptChar;
        type = promptChar.type;
        Update(); // need to force update to enable drawInput calls below to work
        if (type == CharType.Draw) 
        {
            drawInput.Reset();
            drawInput.Init(promptChar.kanjiData);
        }
    }

}


