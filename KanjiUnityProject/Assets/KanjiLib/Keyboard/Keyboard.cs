using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Keyboard : MonoBehaviour
{

    public enum Type 
    {
        Draw,
        FlickRomaji,
        FlickHiragana,
        FlickKatana,
    }

    // keyboard 
    [SerializeField]
    private Type type;

    // refs
    [SerializeField]
    private FlickLayout hiraganaFlickInput;
    [SerializeField]
    private FlickLayout katakanaFlickInput;
    [SerializeField]
    private FlickLayout romajiFlickInput;
    [SerializeField]
    private Kanji2D drawInput;
    [SerializeField]
    public KanjiManager kanjiMan;


    private void Start()
    {
        // flick layout setup
        hiraganaFlickInput.keyboard = this;
        hiraganaFlickInput.Init();
        katakanaFlickInput.keyboard = this;
        katakanaFlickInput.Init();
        romajiFlickInput.keyboard = this;
        romajiFlickInput.Init();

        // needs KanjiData from the keyboard
        //drawInput;

    }

    private void Update()
    {
        switch (type) 
        {
            case Type.Draw:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;
            case Type.FlickHiragana:
                hiraganaFlickInput.gameObject.SetActive(true);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                break;
            case Type.FlickKatana:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(true);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                break;
            case Type.FlickRomaji:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(true);
                drawInput.gameObject.SetActive(false);
                break;
        }
    }

    // called from the flicklayouts and the Kanji2D input mechanisms
    public void UpdateCharacter(string character) 
    {
        kanjiMan.UpdateKanjiPrompt(character);
    }

}


