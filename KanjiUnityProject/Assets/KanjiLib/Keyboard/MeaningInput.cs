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

    void Start()
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
        List<string> fillerMeanings = keyboard.kanjiMan.GetRandomMeanings(
            noOfButtons, 
            promptWord.meanings[0]);
        correctMeaningIdx = Random.Range(0, noOfButtons);
        for (int i = 0; i < meaningButtons.Length; i++)
        {
            meaningButtons[i].text = i == correctMeaningIdx ? 
                promptWord.meanings[0] : 
                fillerMeanings[i];
        }
    }

    public void UpdateWordMeaning(string wordMeaning) 
    {
        if(meaningButtons[correctMeaningIdx].text == wordMeaning) 
        {
            keyboard.WordCompleted();
        }
    }         

}
