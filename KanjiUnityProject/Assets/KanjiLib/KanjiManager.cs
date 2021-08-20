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
    private KanjiTraceable selKanjiTraceable = null;
    private List<KanjiTraceable> kanjiTraceables = new List<KanjiTraceable>();

    // database
    public KanjiDatabase database;
    public TextAsset dataBaseFile;

    // ui 
    public GameObject reticule;
    private RectTransform reticuleTransform;
    public float reticuleRotationrate = 0.12f;

    private void Awake()
    {
        database = new KanjiDatabase();
        database.Load(dataBaseFile);
        reticuleTransform = reticule.GetComponent<RectTransform>();
    }

    void Update()
    {
        UpdateSelection();
        UpdateReticule();
    }


    // give the kanji holder sentence to hold
    public void UpdateKanjiHolder(string character)
    {
        // Apply damage once the kanji is completed

        // flick input


        // kanji writing input
        // remove the kanji display once the selected kanji is destroyed

    }

    public void RegisterKanjiTraceable(KanjiTraceable kanjiTraceable) 
    {
        kanjiTraceables.Add(kanjiTraceable);
    }


    #region selection

    private void UpdateSelection() 
    {
        // see if user selected a kanji holder object
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                var kanjiHolder = hitInfo.collider.gameObject.GetComponent<KanjiTraceable>();
                if (kanjiHolder != null)
                {
                    selKanjiTraceable = kanjiHolder;
                }
            }
        }
    }

    private void UpdateReticule() 
    {
        if(selKanjiTraceable != null && !selKanjiTraceable.IsDestroyed()) 
        {
            reticule.SetActive(true);
            reticuleTransform.position = 
                Camera.main.WorldToScreenPoint(selKanjiTraceable.transform.position);
            reticuleTransform.Rotate(Vector3.forward, reticuleRotationrate*Time.deltaTime);
        }
        else 
        {
            reticule.SetActive(false);
        }
    }

    #endregion

}

