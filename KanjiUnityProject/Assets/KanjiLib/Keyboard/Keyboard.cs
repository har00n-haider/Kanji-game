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

    private void UpdateDisplayString() 
    {
        displayTextMesh.text = currWord?.GetDisplayString();
    }

    private void Reset()
    {
        currWord = null;
        displayTextMesh.text = string.Empty;
    }

    private void ShowInputForType(InputType type) 
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
            case InputType.KeyHiraganaWithRomaji:
            case InputType.KeyKatakanaWithRomaji:
                drawInput.gameObject.SetActive(false);
                flickInput.gameObject.SetActive(true);
                flickInput.SetType(type);
                break;
        }
    }

    // TODO: should use an interface between the two types of input
    private void SetPromptChar(PromptChar promptChar)
    {
        switch (currWord.responseType)
        {
            case InputType.WritingHiragana:
            case InputType.WritingKatakana:
            case InputType.WritingKanji:
                drawInput.SetPromptChar(promptChar);
                break;
            case InputType.KeyHiragana:
            case InputType.KeyKatakana:
            case InputType.KeyKatakanaWithRomaji:
            case InputType.KeyHiraganaWithRomaji:
                flickInput.SetPromptChar(promptChar);
                break;
        }
    }

    // progresses through the prompt word
    public void CharUpdated(char character) 
    {
        if (currWord == null) return; 
        bool passed = currWord.CheckChar(character);
        if (passed) 
        {
            UpdateDisplayString();
            if (currWord.WordCompleted()) 
            {
                Reset();
                kanjiMan.UpdateCurrentKanjiTraceable();     
            }
            else 
            {
                SetPromptChar(currWord.GetChar());
            }
        }
    }

    public void SetPromptWord(PromptWord promptWord) 
    {
        currWord = promptWord;
        // set the prompt char to the relevant input
        ShowInputForType(currWord.responseType);
        SetPromptChar(promptWord.GetChar());
        UpdateDisplayString();
    }
}


