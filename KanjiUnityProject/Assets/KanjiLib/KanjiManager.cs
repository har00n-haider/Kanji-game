using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages objects in the scene that hold kanji prompts
/// 
/// Middle man between the input <-> kanjiprompt
/// </summary>
public class KanjiManager : MonoBehaviour
{
    // scoring
    // the number of times a character has be succesfully input
    // before it the reference strokes are hidden
    public static readonly int hideWritingRefThreshold = 3;

    // kanji holder managment
    private KanjiTraceable selectedKanjiTraceable = null;
    private List<KanjiTraceable> kanjiTraceables = new List<KanjiTraceable>();

    // database
    public KanjiDatabase database;
    public TextAsset kanjiDataBaseFile;
    public TextAsset sentenceDataBaseFile;

    // ui 
    public GameObject reticule;
    private RectTransform reticuleTransform;
    public float reticuleRotationrate = 0.12f;

    // refs
    Keyboard keyboard;

    private void Awake()
    {
        database = new KanjiDatabase();
        database.Load(kanjiDataBaseFile, sentenceDataBaseFile);
        reticuleTransform = reticule.GetComponent<RectTransform>();
        keyboard = GameObject.FindGameObjectWithTag("Keyboard").GetComponent<Keyboard>();
    }

    void Update()
    {
        CheckSelectionInput();
        UpdateReticule();
    }

    // called by the keyboard
    public void UpdateCurrentKanjiTraceable()
    {
        bool completed = selectedKanjiTraceable.MoveNext();
        if (completed) 
        {
            selectedKanjiTraceable.Destroy();
        }
        else 
        {
            keyboard.SetPromptWord(selectedKanjiTraceable.currWord);
        }
    }

    public void RegisterKanjiTraceable(KanjiTraceable kanjiTraceable) 
    {
        kanjiTraceables.Add(kanjiTraceable);
        kanjiTraceable.prompt = GeneratePrompt();
    }

    private void UpdateSelection(KanjiTraceable selectedKanji) 
    {
        selectedKanjiTraceable = selectedKanji;
        keyboard.SetPromptWord(selectedKanji.currWord);
    }

    private Prompt GeneratePrompt()
    {
        return database.GetRandomPrompt();
    }

    #region selection

    private void CheckSelectionInput() 
    {
        // see if user selected a kanji holder object
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                var selectedKanji = hitInfo.collider.gameObject.GetComponent<KanjiTraceable>();
                if (selectedKanji != null)
                {
                    UpdateSelection(selectedKanji);
                }
            }
        }
    }

    private void UpdateReticule() 
    {
        if(selectedKanjiTraceable != null && !selectedKanjiTraceable.IsDestroyed()) 
        {
            reticule.SetActive(true);
            reticuleTransform.position = 
                Camera.main.WorldToScreenPoint(selectedKanjiTraceable.transform.position);
            reticuleTransform.Rotate(Vector3.forward, reticuleRotationrate*Time.deltaTime);
        }
        else 
        {
            reticule.SetActive(false);
        }
    }

    #endregion

}

