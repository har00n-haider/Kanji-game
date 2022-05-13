using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using KanjiLib.Utils;
using KanjiLib.Prompts;


namespace KanjiLib.Core
{

/// <summary>
/// Manages objects in the scene that hold kanji prompts
///
/// Middle man between the keyboard <-> promptholders
/// </summary>
public class KanjiManager : MonoBehaviour
{
    // scoring
    // the number of times a character has be succesfully input
    // before it the reference strokes are hidden
    public static readonly int hideWritingRefThreshold = 3;

    // kanji holder managment
    private PromptHolder selectedPromptHolder = null;

    private List<PromptHolder> promptHolders = new List<PromptHolder>();

    // database
    public KanjiDatabase database;

    public TextAsset kanjiDataBaseFile;
    public TextAsset sentenceDataBaseFile;


    private void Awake()
    {
        database = new KanjiDatabase();
        database.Load(kanjiDataBaseFile, sentenceDataBaseFile);
    }

    private void Update()
    {
        UpdateSelectedPromptHolderFromScene();
    }

    // called by the keyboard
    public void UpdateCurrentPromptHolder()
    {
        if (selectedPromptHolder == null) return;
        selectedPromptHolder.MoveNext();
        if (!selectedPromptHolder.completed)
        {
            //keyboard.SetPromptWord(selectedPromptHolder.GetCurrentWord());
        }
        else
        {
            AutoTarget();
        }
    }

    public void RegisterPromptHolder(PromptHolder promptHolder)
    {
        promptHolders.Add(promptHolder);
    }

    public void RemovePromptHolder(PromptHolder promptHolder)
    {
        promptHolders.Remove(promptHolder);
    }

    public List<string> GetRandomMeanings(int noOfMeanings, string except = null)
    {
        return database.GetRandomFillerMeanings(noOfMeanings, except);
    }

    #region prompt setup

#if UNITY_EDITOR

    private int pIdx = -1;

    // TODO: debug function
    private Prompt GetNextPrompt()
    {
        ++pIdx;
        var prompt = database.GetPromptById(pIdx);
        foreach (var word in prompt.words)
        {
            GetTestSetForWordType(
                word.type,
                out PromptDisplayType displayType,
                out PromptInputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    // TODO: debug function, manually written based on what you want to test
    private void GetTestSetForWordType(
    PromptWord.WordType promptType,
        out PromptDisplayType displayType,
        out PromptInputType responseType)
    {
        displayType = PromptDisplayType.Kanji;
        responseType = PromptInputType.KeyHiragana;

        switch (promptType)
        {
            case PromptWord.WordType.kanji:
                displayType = PromptDisplayType.Kanji;
                responseType = PromptInputType.Meaning;
                break;

            case PromptWord.WordType.hiragana:
                displayType = PromptDisplayType.Hiragana;
                responseType = PromptInputType.KeyHiraganaWithRomaji;
                break;

            case PromptWord.WordType.katakana:
                displayType = PromptDisplayType.Katana;
                responseType = PromptInputType.KeyKatakanaWithRomaji;
                break;

            default:
                break;
        }
    }

#endif

    public Prompt GetPrompt(PromptConfiguration promptConfig)
    {
        Prompt prompt = database.GetPrompt(promptConfig);
        foreach (var word in prompt.words)
        {
            word.responseType = promptConfig.responseType;
            word.displayType = promptConfig.displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    private Prompt GetRandomPrompt()
    {
        Prompt prompt = database.GetRandomPrompt();
        foreach (var word in prompt.words)
        {
            GetRandomTestSetForWordType(
                word.type,
                out PromptDisplayType displayType,
                out PromptInputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    private void GetRandomTestSetForWordType(
        PromptWord.WordType promptType,
        out PromptDisplayType displayType,
        out PromptInputType responseType)
    {
        displayType = PromptDisplayType.Kanji;
        responseType = PromptInputType.KeyHiragana;

        switch (promptType)
        {
            case PromptWord.WordType.kanji:
                displayType = Prompts.Utils.kanjiPrompts.GetRandomPrompt();
                responseType = Prompts.Utils.kanjiInputs.GetRandomInput();
                break;

            case PromptWord.WordType.hiragana:
                displayType =  Prompts.Utils.hiraganaPrompts.GetRandomPrompt();
                responseType = Prompts.Utils.hiraganaInputs.GetRandomInput();
                break;

            case PromptWord.WordType.katakana:
                displayType =  Prompts.Utils.katakanaPrompts.GetRandomPrompt();
                responseType = Prompts.Utils.katakanaInputs.GetRandomInput();
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// This function should be run out of the database it depends
    /// on the prompt having been configured for a test
    /// </summary>
    /// <param name="prompt">Prompt that has been configured for a test</param>
    private void SetCharsForPrompt(ref Prompt prompt)
    {
        Action<List<PromptChar>, string> populateCharList =
        (List<PromptChar> cl, string s) =>
        {
            foreach (char c in s)
            {
                cl.Add(new PromptChar()
                {
                    character = c,
                    data = database.GetKanji(c)
                });
            }
        };

        // Set the chars to iterate through depending
        // on the type of the word and the input type
        foreach (PromptWord word in prompt.words)
        {
            List<PromptChar> chars = new List<PromptChar>();
            switch (word.type)
            {
                case PromptWord.WordType.kanji:
                    // take the input type into consideration
                    // for kanji as it could go multpile ways
                    switch (word.responseType)
                    {
                        case PromptInputType.KeyHiraganaWithRomaji:
                        case PromptInputType.KeyHiragana:
                        case PromptInputType.WritingHiragana:
                            populateCharList(chars, word.hiragana);
                            break;

                        case PromptInputType.WritingKanji:
                        case PromptInputType.Meaning:
                            populateCharList(chars, word.kanji);
                            break;
                    }
                    break;
                // hiragana/katana will always only have their own char type
                case PromptWord.WordType.hiragana:
                    populateCharList(chars, word.hiragana);
                    break;

                case PromptWord.WordType.katakana:
                    populateCharList(chars, word.katakana);
                    break;
            }
            word.chars = chars.ToArray();
        }
    }

    #endregion prompt setup

    #region selection

    private void AutoTarget()
    {
        if (promptHolders.Count == 0) return;
        // populate array of distances
        Tuple<float, PromptHolder>[] distToPromptHolder = new Tuple<float, PromptHolder>[promptHolders.Count];
        for (int i = 0; i < promptHolders.Count; i++)
        {
            distToPromptHolder[i] = new Tuple<float, PromptHolder>
            (
                2.0f, //(mainCharacter.transform.position - promptHolders[i].transform.position).magnitude,
                promptHolders[i]
            );
        }
        // select the closest
        var closestPromptHolder = distToPromptHolder.Aggregate((curMin, x) =>
                (curMin == null || x.Item1 < curMin.Item1 ? x : curMin)).Item2;
        UpdateSelection(closestPromptHolder);
    }

    private void UpdateSelection(PromptHolder selectedKanji)
    {
        selectedPromptHolder = selectedKanji;
    }

    // unity event method
    // So that we can get select the prompt from the label aswell
    public void UpdateSelectedPromptHolderFromLabel(PromptHolder selectedPrompt)
    {
        if (selectedPrompt != null)
        {
            UpdateSelection(selectedPrompt);
        }
    }

    private void UpdateSelectedPromptHolderFromScene()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                var selectedKanji = hitInfo.collider.gameObject.GetComponent<PromptHolder>();
                if (selectedKanji != null)
                {
                    UpdateSelection(selectedKanji);
                }
            }
        }
    }

    #endregion selection
}

}