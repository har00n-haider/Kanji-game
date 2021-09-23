﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PromptHolder : MonoBehaviour
{
    public bool selected { get; set; }

    // label config
    public Color labelColor;

    public Color textColor;
    public float labelOffsetYPercentage;
    private bool setColliderSize = false;

    // prompt string content
    private Prompt prompt { get; set; }

    // prompt string configuration
    private Color completedColor = Color.grey;

    private Color hiraganaColor = Color.red;
    private Color katanaColor = Color.yellow;
    private Color kanjiColor = Color.blue;
    private Color romajiColor = Color.green;

    // refs
    private RectTransform labelRect;

    private TextMeshProUGUI textMesh;

    [SerializeField]
    private GameObject labelPrefab;

    private KanjiManager kanjiMan;
    public IPromptHolderControllable controlledGameObject;

    private void Awake()
    {
        controlledGameObject = GetComponent<IPromptHolderControllable>();
        kanjiMan = GameObject.FindGameObjectWithTag("KanjiManager").GetComponent<KanjiManager>();

        // always place the label on the main canvas
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        GameObject label = Instantiate(labelPrefab, mainCanvas.transform);
        labelRect = label.GetComponentInChildren<RectTransform>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        kanjiMan.RegisterPromptHolder(this);
        controlledGameObject.SetHealth(prompt.words.Count);
        UpdateLabelScreenPos(labelRect, labelOffsetYPercentage);
    }

    // Update is called once per frame
    private void Update()
    {
        // the label takes a frame or so to get the right size (mayebe the content fitter?)
        if (!setColliderSize && labelRect?.rect.size.magnitude > 0)
        {
            ConfigureLabel(labelRect.gameObject);
            setColliderSize = true;
        }
        UpdateLabelScreenPos(labelRect, labelOffsetYPercentage);
    }

    public bool IsDestroyed()
    {
        return gameObject == null;
    }

    private void ConfigureLabel(GameObject label)
    {
        textMesh = label.GetComponentInChildren<TextMeshProUGUI>();
        textMesh.font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Regular SDF");
        SetTextMesh();
        var labelImage = label.GetComponentInChildren<Image>();
        labelImage.color = labelColor;
        var labelBoxCollider = label.GetComponent<BoxCollider2D>();
        labelBoxCollider.size = labelRect.rect.size;
    }

    private void UpdateLabelScreenPos(RectTransform lRect, float yOffsetPercentage)
    {
        if (lRect == null) return;
        float labelRectW = lRect.rect.width;
        float labelRectH = lRect.rect.height;
        float sH = Camera.main.pixelHeight;
        float sW = Camera.main.pixelWidth;
        // update the location of the label on the screen
        var screenpoint = Camera.main.WorldToScreenPoint(transform.position);
        // smooth clamp rect to the screen dimensions
        float labelY = GeometryUtils.ClampLengthToRegion(screenpoint.y, labelRectH, sH);
        float labelX = GeometryUtils.ClampLengthToRegion(screenpoint.x, labelRectW, sW);
        // apply vertical offset to label
        float labelYOffset = yOffsetPercentage * sH;
        lRect.position = new Vector2(labelX, labelY + labelYOffset);
    }

    private void SetTextMesh()
    {
        string textMeshText = "";
        for (int i = 0; i < prompt.words.Count; i++)
        {
            PromptWord pw = prompt.words[i];
            // Get color
            Color color = Color.white;
            if (pw.Completed())
            {
                color = completedColor;
            }
            else
            {
                switch (pw.displayType)
                {
                    case PromptType.Kanji:
                        color = kanjiColor;
                        break;

                    case PromptType.Hiragana:
                        color = hiraganaColor;
                        break;

                    case PromptType.Katana:
                        color = katanaColor;
                        break;

                    case PromptType.Romaji:
                        color = romajiColor;
                        break;

                    case PromptType.Meaning:
                    default:
                        break;
                }
            }
            // Get text
            string text = string.Empty;
            switch (pw.displayType)
            {
                case PromptType.Kanji:
                    text = pw.kanji;
                    break;

                case PromptType.Hiragana:
                    text = pw.hiragana;
                    break;

                case PromptType.Katana:
                    text = pw.katakana;
                    break;

                case PromptType.Romaji:
                    text = WanaKanaSharp.WanaKana.ToRomaji(pw.katakana);
                    break;

                case PromptType.Meaning:
                default:
                    break;
            }

            textMeshText += $"{text.AddColor(color)}";
        }
        textMesh.SetText(textMeshText);
    }

    public bool MoveNext()
    {
        prompt.MoveNext();
        SetTextMesh();
        if (prompt.Completed()) Destroy(labelRect.gameObject);
        return prompt.Completed();
    }

    public bool Completed()
    {
        return prompt.Completed();
    }

    public PromptWord GetCurrentWord()
    {
        return prompt.currWord;
    }

    public void SetPrompt(Prompt prompt)
    {
        this.prompt = prompt;
    }
}