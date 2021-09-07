using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Keyboard : MonoBehaviour
{
    // refs
    [SerializeField]
    private FlickLayout flickInput;
    [SerializeField]
    private Kanji2D drawInput;
    [SerializeField]
    private TextMeshProUGUI displayTextMesh;

    private KanjiManager kanjiMan;

    // state
    public PromptWord currWord { get; set; } = null;
    private int charIdx = 0;
    private StringBuilder displayStr = new StringBuilder();

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

    private void Reset()
    {
        currWord = null;
        charIdx = 0;
        displayStr.Clear();
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

    public void CharUpdatedSuccesfully() 
    {
        displayStr[charIdx] = currWord.chars[charIdx].character;
        displayTextMesh.text = displayStr.ToString();
        if (charIdx + 1 < currWord.chars.Length) 
        {
            charIdx++;
            flickInput.inputHandler.SetPromptChar(currWord.chars[charIdx]);
        }
        else 
        {
            Reset();
            kanjiMan.UpdateCurrentKanjiTraceable();     
        }
    }

    public void SetPromptWord(PromptWord promptWord) 
    {
        currWord = promptWord;
        // fill the display string
        displayStr.Clear();
        foreach(PromptChar p in currWord.chars) 
        {
            displayStr.Append('☐');
        }
        displayTextMesh.text = displayStr.ToString();
        // set the prompt char to the relevant input
        switch (currWord.responseType)
        {
            case InputType.WritingHiragana:
            case InputType.WritingKatakana:
            case InputType.WritingKanji:
                drawInput.SetPromptChar(currWord.chars[charIdx]);
                break;
            case InputType.KeyHiragana:
            case InputType.KeyKatakana:
            case InputType.KeyRomaji:
                flickInput.inputHandler.SetPromptChar(currWord.chars[charIdx]);
                break;
        }
        SetInputType(currWord.responseType);
    }
}


