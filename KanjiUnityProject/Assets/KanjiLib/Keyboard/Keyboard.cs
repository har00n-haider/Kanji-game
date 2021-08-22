using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Keyboard : MonoBehaviour
{
    // keyboard 
    [SerializeField]
    private CharType type;

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

    public PromptChar currCharTarget { get; set; }

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
        //drawInput;

    }

    private void Update()
    {
        switch (type) 
        {
            case CharType.Draw:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;
            case CharType.FlickHiragana:
                hiraganaFlickInput.gameObject.SetActive(true);
                katakanaFlickInput.gameObject.SetActive(false);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                break;
            case CharType.FlickKatana:
                hiraganaFlickInput.gameObject.SetActive(false);
                katakanaFlickInput.gameObject.SetActive(true);
                romajiFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                break;
            case CharType.FlickRomaji:
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
        Debug.Log(character);
        if (character == currCharTarget.character)
        {
            kanjiMan.UpdateCurrentKanjiTraceable(true);
        }
    }

}


