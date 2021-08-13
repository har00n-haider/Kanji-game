using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages what objects in the scene hold kanjis
/// and what kanji to display on screen.
/// Should be the central place to manage how the 
/// kanji interact with the rest of the game
/// 
/// Deals with the connection between Kanji holder and kanji object
/// </summary>
public class KanjiManager : MonoBehaviour
{
    // settings for scoring behaviour
    public static readonly int hideReferenceThreshold = 3;

    private Kanji3D inputKanji;
    private IKanjiHolder selectedKanjiHolder = null;
    public Kanji3D kanjiPrefab;

    public KanjiDatabase database;
    public TextAsset dataBaseFile;

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
        // see if user selected a kanji holder object
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                var kanjiHolder = hitInfo.collider.gameObject.GetComponent<IKanjiHolder>();
                if (kanjiHolder != null)
                {
                    selectedKanjiHolder = kanjiHolder;
                    UpdateInputKanji(selectedKanjiHolder.kanjiData);
                }
            }
        }

        // Apply damage once the kanji is completed
        if (inputKanji != null && inputKanji.completed)
        {
            selectedKanjiHolder.TakeDamage(inputKanji.score);
            Destroy(inputKanji.gameObject);
            inputKanji = null;
        }

        // remove the kanji display once the selected kanji is destroyed
        if (selectedKanjiHolder != null && selectedKanjiHolder.IsDestroyed())
        {
            if (inputKanji != null) Destroy(inputKanji.gameObject);
        }

        UpdateReticule();
    }

    public void UpdateInputKanji(KanjiData kanjiData)
    {
        if (inputKanji != null) Destroy(inputKanji.gameObject);
        var kanji = Instantiate(kanjiPrefab, transform).GetComponent<Kanji3D>();
        kanji.Init(kanjiData);
        inputKanji = kanji;
    }


    private void UpdateReticule() 
    {
        if(selectedKanjiHolder != null && !selectedKanjiHolder.IsDestroyed()) 
        {
            reticule.SetActive(true);
            reticuleTransform.position = 
                Camera.main.WorldToScreenPoint(selectedKanjiHolder.transform.position);
            reticuleTransform.Rotate(Vector3.forward, reticuleRotationrate*Time.deltaTime);
        }
        else 
        {
            reticule.SetActive(false);
        }
    }
}

