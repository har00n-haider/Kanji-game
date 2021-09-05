using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Keyboard : MonoBehaviour
{
    // refs
    [SerializeField]
    private FlickLayout flickInput;
    [SerializeField]
    private Kanji2D drawInput;

    private KanjiManager kanjiMan;

    private PromptWord currCharTarget { get; set; } = null;
    private int charIdx = 0;

    private void Awake() 
    { 
        kanjiMan = GameObject.FindGameObjectWithTag("KanjiManager").GetComponent<KanjiManager>();
    }

    private void Start()
    {
        // flick layout setup
        flickInput.keyboard = this;
        flickInput.Init();

        // needs KanjiData from the kanjimanager , i.e. the current kanji to draw
        drawInput.keyboard = this;
    }

    private void Update()
    {
    }

    private void SetInputType(InputType type) 
    {
        switch (type)
        {
            case InputType.WritingHiragana:
            case InputType.WritingKatakana:
            case InputType.WritingKanji:
                flickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;
            case InputType.KeyHiragana:
            case InputType.KeyKatakana:
            case InputType.KeyRomaji:
                flickInput.gameObject.SetActive(true);
                drawInput.gameObject.SetActive(false);
                break;
        }
        flickInput.SetType(type);
    }

    // called from the flicklayouts and the Kanji2D input methods
    public void CharCompletedSuccesfully() 
    {
        kanjiMan.UpdateCurrentKanjiTraceable();
    }

    public void SetPromptWord(PromptWord promptWord) 
    {
        switch (promptWord.responseType)
        {
            case InputType.WritingHiragana:
            case InputType.WritingKatakana:
            case InputType.WritingKanji:
                SetInputType(promptWord.responseType);
                drawInput.SetPromptChar(promptWord.chars[charIdx]);
                break;
            case InputType.KeyHiragana:
            case InputType.KeyKatakana:
            case InputType.KeyRomaji:
                SetInputType(promptWord.responseType);
                flickInput.SetPromptChar(promptWord.chars[charIdx]);
                break;
        }
    }
}


