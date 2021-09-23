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

    [SerializeField]
    private MeaningInput meaningInput;

    public KanjiManager kanjiMan;

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
        meaningInput.keyboard = this;
    }

    private void UpdateDisplayString()
    {
        if (currWord != null)
        {
            if (currWord.responseType == InputType.Meaning)
            {
                displayTextMesh.text = currWord.ToString();
            }
            else
            {
                bool displayRomaji =
                    currWord.responseType == InputType.KeyHiraganaWithRomaji ||
                    currWord.responseType == InputType.KeyKatakanaWithRomaji;
                string originalString = "<mspace=1em>" + currWord.GetDisplayString() + "</mspace>";
                displayTextMesh.text = !displayRomaji ? originalString :
                    WanaKanaSharp.WanaKana.ToRomaji(currWord.GetCompletedKanaString()) + "\n" + originalString;
            }
        }
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
                meaningInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;

            case InputType.KeyHiragana:
            case InputType.KeyKatakana:
            case InputType.KeyHiraganaWithRomaji:
            case InputType.KeyKatakanaWithRomaji:
                meaningInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                flickInput.gameObject.SetActive(true);
                flickInput.SetType(type);
                break;

            case InputType.Meaning:
                drawInput.gameObject.SetActive(false);
                flickInput.gameObject.SetActive(false);
                meaningInput.gameObject.SetActive(true);
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
        // fire the word completed only once
        if (!currWord.Completed() && currWord.CheckChar(character))
        {
            UpdateDisplayString();
            if (currWord.Completed())
            {
                WordCompleted();
            }
            else
            {
                SetPromptChar(currWord.GetChar());
            }
        }
    }

    public void WordCompleted()
    {
        Reset();
        kanjiMan.UpdateCurrentPromptHolder();
    }

    public void SetPromptWord(PromptWord promptWord)
    {
        currWord = promptWord;
        // set the prompt char to the relevant input
        ShowInputForType(currWord.responseType);
        if (currWord.responseType == InputType.Meaning)
        {
            meaningInput.SetPromptWord(promptWord);
        }
        else
        {
            SetPromptChar(promptWord.GetChar());
        }
        UpdateDisplayString();
    }
}