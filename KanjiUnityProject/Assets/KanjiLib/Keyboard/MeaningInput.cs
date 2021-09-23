using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeaningInput : MonoBehaviour
{
    // configuration
    public int noOfButtons = 3;

    // state
    private int correctMeaningIdx;

    // refs
    [SerializeField]
    private GameObject meaningButtonPrefab;

    private MeaningButton[] meaningButtons;

    [HideInInspector]
    public Keyboard keyboard;

    private PromptWord promptWord;

    private void Start()
    {
        meaningButtons = new MeaningButton[noOfButtons];
        for (int i = 0; i < noOfButtons; i++)
        {
            MeaningButton button = Instantiate(meaningButtonPrefab, transform).GetComponent<MeaningButton>();
            button.meaningInput = this;
            button.Init();
            meaningButtons[i] = button;
        }
    }

    public void SetPromptWord(PromptWord promptWord)
    {
        this.promptWord = promptWord;
        List<string> fillerMeanings = keyboard.kanjiMan.GetRandomMeanings(
            noOfButtons,
            promptWord.meanings[0]);
        correctMeaningIdx = Random.Range(0, noOfButtons);
        for (int i = 0; i < meaningButtons.Length; i++)
        {
            meaningButtons[i].text = i == correctMeaningIdx ?
                promptWord.GetMeaning() :
                fillerMeanings[i];
        }
    }

    public void UpdateWordMeaning(string wordMeaning)
    {
        // fire the word completed only once
        if (!promptWord.Completed() && promptWord.CheckMeaning(wordMeaning))
        {
            keyboard.WordCompleted();
        }
    }
}