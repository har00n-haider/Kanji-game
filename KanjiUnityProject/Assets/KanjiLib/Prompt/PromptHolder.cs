using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using KanjiLib.Core;

/// <summary>
/// Exposes the game object it is attached to, to the prompt system
/// </summary>
public class PromptHolder : MonoBehaviour
{
    // state
    public bool completed { get; private set; } = false;

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
        controlledGameObject.onDestroy += HandleOnDestroy;
        kanjiMan = GameObject.FindGameObjectWithTag("KanjiManager").GetComponent<KanjiManager>();

        // always place the label on the main canvas
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        // set up the label
        PromptLabel label = Instantiate(labelPrefab, mainCanvas.transform).GetComponent<PromptLabel>();
        label.kanjiManager = kanjiMan;
        label.promptHolder = this;
        labelRect = label.gameObject.GetComponentInChildren<RectTransform>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        kanjiMan.RegisterPromptHolder(this);

        // Always assume that there is at least one prompt
        UpdatePrompt(controlledGameObject.getPromptConfig);

        UIUtils.UpdateLabelScreenPos(labelRect, labelOffsetYPercentage, transform.position);
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
        UIUtils.UpdateLabelScreenPos(labelRect, labelOffsetYPercentage, transform.position);
    }

    private void UpdatePrompt(PromptConfiguration promptConfiguration)
    {
        // Get a prompt that matches the configuration that the game object wants
        prompt = kanjiMan.GetPrompt(promptConfiguration);
        // Inform the controlled object what prompt was chosen (there may be
        // game design implications)
        controlledGameObject.OnCurrentPromptSet(prompt);
        controlledGameObject.AddHealth(prompt.words.Count);
        ConfigureLabel(labelRect.gameObject);
    }

    public bool IsDestroyed()
    {
        return gameObject == null;
    }

    #region label management

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
                    case PromptDisplayType.Kanji:
                        color = kanjiColor;
                        break;

                    case PromptDisplayType.Hiragana:
                        color = hiraganaColor;
                        break;

                    case PromptDisplayType.Katana:
                        color = katanaColor;
                        break;

                    case PromptDisplayType.Romaji:
                        color = romajiColor;
                        break;

                    case PromptDisplayType.Meaning:
                    default:
                        break;
                }
            }
            // Get text
            string text = string.Empty;
            switch (pw.displayType)
            {
                case PromptDisplayType.Kanji:
                    text = pw.kanji;
                    break;

                case PromptDisplayType.Hiragana:
                    text = pw.hiragana;
                    break;

                case PromptDisplayType.Katana:
                    text = pw.katakana;
                    break;

                case PromptDisplayType.Romaji:
                    text = WanaKanaSharp.WanaKana.ToRomaji(pw.katakana);
                    break;

                case PromptDisplayType.Meaning:
                default:
                    break;
            }

            textMeshText += $"{text.AddColor(color)}";
        }
        textMesh.SetText(textMeshText);
    }

    #endregion label management

    public bool MoveNext()
    {
        prompt.MoveNext();
        SetTextMesh();
        if (prompt.Completed())
        {
            // current prompt completed try to get another
            PromptConfiguration config = controlledGameObject.getPromptConfig;
            if (config != null)
            {
                UpdatePrompt(config);
            }
            else
            {
                completed = true;
                kanjiMan.RemovePromptHolder(this);
            }
        }
        return prompt.Completed();
    }

    public PromptWord GetCurrentWord()
    {
        return prompt.currWord;
    }

    // Used to clean up the prompt holder (labels etc.)
    private void HandleOnDestroy()
    {
        kanjiMan.RemovePromptHolder(this);
        if (labelRect != null) Destroy(labelRect.gameObject);
    }
}