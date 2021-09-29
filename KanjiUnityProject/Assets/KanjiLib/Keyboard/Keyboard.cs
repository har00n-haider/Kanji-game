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
            if (currWord.responseType == PromptInputType.Meaning)
            {
                displayTextMesh.text = currWord.ToString();
            }
            else
            {
                bool displayRomaji =
                    currWord.responseType == PromptInputType.KeyHiraganaWithRomaji ||
                    currWord.responseType == PromptInputType.KeyKatakanaWithRomaji;
                string originalString = "<mspace=1em>" + currWord.GetDisplayString() + "</mspace>";
                displayTextMesh.text = !displayRomaji ? originalString :
                    WanaKanaSharp.WanaKana.ToRomaji(currWord.GetFullKanaString()) + "\n" + originalString;
            }
        }
    }

    private void Reset()
    {
        currWord = null;
        displayTextMesh.text = string.Empty;
    }

    private void ShowInputForType(PromptInputType type)
    {
        switch (type)
        {
            case PromptInputType.WritingHiragana:
            case PromptInputType.WritingKatakana:
            case PromptInputType.WritingKanji:
                flickInput.gameObject.SetActive(false);
                meaningInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;

            case PromptInputType.KeyHiragana:
            case PromptInputType.KeyKatakana:
            case PromptInputType.KeyHiraganaWithRomaji:
            case PromptInputType.KeyKatakanaWithRomaji:
                meaningInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(false);
                flickInput.gameObject.SetActive(true);
                flickInput.SetType(type);
                break;

            case PromptInputType.Meaning:
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
            case PromptInputType.WritingHiragana:
            case PromptInputType.WritingKatakana:
            case PromptInputType.WritingKanji:
                drawInput.SetPromptChar(promptChar);
                break;

            case PromptInputType.KeyHiragana:
            case PromptInputType.KeyKatakana:
            case PromptInputType.KeyKatakanaWithRomaji:
            case PromptInputType.KeyHiraganaWithRomaji:
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
        if (currWord.responseType == PromptInputType.Meaning)
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