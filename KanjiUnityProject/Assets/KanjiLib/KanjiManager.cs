using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

    // ui
    public GameObject reticule;

    private RectTransform reticuleTransform;
    public float reticuleRotationrate = 0.12f;

    // refs
    private Keyboard keyboard;

    private void Awake()
    {
        database = new KanjiDatabase();
        database.Load(kanjiDataBaseFile, sentenceDataBaseFile);
        reticuleTransform = reticule.GetComponent<RectTransform>();
        keyboard = GameObject.FindGameObjectWithTag("Keyboard").GetComponent<Keyboard>();
    }

    private void Update()
    {
        CheckSelectionInput();
        UpdateReticule();
    }

    // called by the keyboard
    public void UpdateCurrentPromptHolder()
    {
        bool completed = selectedPromptHolder.MoveNext();
        if (completed)
        {
            selectedPromptHolder.Destroy();
        }
        else
        {
            keyboard.SetPromptWord(selectedPromptHolder.currWord);
        }
    }

    public void RegisterPromptHolder(PromptHolder promptHolder)
    {
        promptHolders.Add(promptHolder);
        promptHolder.prompt = GetNextPrompt();
    }

    public List<string> GetRandomMeanings(int noOfMeanings, string except = null)
    {
        return database.GetRandomFillerMeanings(noOfMeanings, except);
    }

    #region prompt setup

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
                out PromptType displayType,
                out InputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    // TODO: debug function, manually written based on what you want to test
    private void GetTestSetForWordType(
    PromptWord.WordType promptType,
        out PromptType displayType,
        out InputType responseType)
    {
        displayType = PromptType.Kanji;
        responseType = InputType.KeyHiragana;

        switch (promptType)
        {
            case PromptWord.WordType.kanji:
                displayType = PromptType.Kanji;
                responseType = InputType.Meaning;
                break;

            case PromptWord.WordType.hiragana:
                displayType = PromptType.Hiragana;
                responseType = InputType.KeyHiraganaWithRomaji;
                break;

            case PromptWord.WordType.katakana:
                displayType = PromptType.Katana;
                responseType = InputType.KeyKatakanaWithRomaji;
                break;

            default:
                break;
        }
    }

    private Prompt GetRandomPrompt()
    {
        Prompt prompt = database.GetRandomPrompt();
        foreach (var word in prompt.words)
        {
            GetRandomTestSetForWordType(
                word.type,
                out PromptType displayType,
                out InputType responseType);
            word.responseType = responseType;
            word.displayType = displayType;
        }
        SetCharsForPrompt(ref prompt);
        return prompt;
    }

    private void GetRandomTestSetForWordType(
        PromptWord.WordType promptType,
        out PromptType displayType,
        out InputType responseType)
    {
        displayType = PromptType.Kanji;
        responseType = InputType.KeyHiragana;

        switch (promptType)
        {
            case PromptWord.WordType.kanji:
                displayType = KanjiUtils.kanjiPrompts.GetRandomPrompt();
                responseType = KanjiUtils.kanjiInputs.GetRandomInput();
                break;

            case PromptWord.WordType.hiragana:
                displayType = KanjiUtils.hiraganaPrompts.GetRandomPrompt();
                responseType = KanjiUtils.hiraganaInputs.GetRandomInput();
                break;

            case PromptWord.WordType.katakana:
                displayType = KanjiUtils.katakanaPrompts.GetRandomPrompt();
                responseType = KanjiUtils.katakanaInputs.GetRandomInput();
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
                        case InputType.KeyHiraganaWithRomaji:
                        case InputType.KeyHiragana:
                        case InputType.WritingHiragana:
                            populateCharList(chars, word.hiragana);
                            break;

                        case InputType.WritingKanji:
                        case InputType.Meaning:
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

    private void UpdateSelection(PromptHolder selectedKanji)
    {
        selectedPromptHolder = selectedKanji;
        keyboard.SetPromptWord(selectedKanji.currWord);
    }

    private void CheckSelectionInput()
    {
        // see if user selected a kanji holder object
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

    private void UpdateReticule()
    {
        if (selectedPromptHolder != null && !selectedPromptHolder.IsDestroyed())
        {
            reticule.SetActive(true);
            reticuleTransform.position =
                Camera.main.WorldToScreenPoint(selectedPromptHolder.transform.position);
            reticuleTransform.Rotate(Vector3.forward, reticuleRotationrate * Time.deltaTime);
        }
        else
        {
            reticule.SetActive(false);
        }
    }

    #endregion selection
}