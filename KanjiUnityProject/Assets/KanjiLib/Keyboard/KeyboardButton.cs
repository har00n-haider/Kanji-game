using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class KeyboardButton : MonoBehaviour
{
    private enum FlickType
    {
        Up,
        Down,
        Left,
        Right,
        Center
    }

    // currently designed to match a hierarchy of
    // - button (top level game object)
    //  - flick button (with image, i.e. instance of this class)
    //   - text mesh 
    //  ... (more flick buttons)
    private class FlickButton
    {
        private RectTransform buttonRect; 
        private RectTransform imageRect;
        private GameObject gameObject;
        private TextMeshProUGUI textMesh;
        private Image image;
        private FlickType type;
        public Vector2 relativePosScale;

        public string character
        {
            get
            {
                string val = " ";
                val = textMesh.text.Length > 0 ? textMesh.text : val;
                return val;
            }
            set
            {
                textMesh.text = value.ToString();
            }
        }

        public FlickButton(GameObject flickButtonGameObject, FlickType type)
        {
            this.type = type;
            this.gameObject = flickButtonGameObject;
            textMesh = flickButtonGameObject.GetComponentInChildren<TextMeshProUGUI>();
            textMesh.font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Regular SDF");
            image = flickButtonGameObject.GetComponent<Image>();
            imageRect = flickButtonGameObject.GetComponent<RectTransform>();
            buttonRect = flickButtonGameObject.GetComponentInParent<RectTransform>();
            relativePosScale.x = buttonRect.rect.width / imageRect.anchoredPosition.x;
            relativePosScale.y = buttonRect.rect.height / imageRect.anchoredPosition.y;
        }

        public void SetFontSize(float fontSize) 
        {
            textMesh.enableAutoSizing = false;
            textMesh.fontSize = fontSize;
        }

        public void SetVisibility(bool value, bool exceptText = false)
        {
            image.enabled = value;
            textMesh.enabled = value;
        }

        public void SetColors(Color imageColor, Color? textColor = null) 
        {
            image.color = imageColor;
            if (textColor.HasValue) 
            {
                textMesh.color =  textColor.Value;
            }
        }

        public void Resize(float distFromCenter, float padding)
        {
            SetRelativePosScale(distFromCenter);
            imageRect.ScalePosRelativeToParentSize(buttonRect, relativePosScale);
            textMesh.rectTransform.SetPadding(padding, padding, padding, padding);
        }

        public void SetActive(bool state) 
        {
            gameObject.SetActive(state);
        }

        private void SetRelativePosScale(float distance) 
        {
            switch (type) 
            {
                case FlickType.Up:
                    relativePosScale.x = 0;
                    relativePosScale.y = distance;
                    break;
                case FlickType.Down:
                    relativePosScale.x = 0;
                    relativePosScale.y = -distance;
                    break;
                case FlickType.Left:
                    relativePosScale.x = -distance;
                    relativePosScale.y = 0;
                    break;
                case FlickType.Right:
                    relativePosScale.x = distance;
                    relativePosScale.y = 0;
                    break;
                case FlickType.Center:
                    break;
            }
        }
    }

    [Serializable]
    public class Config
    {
        [SerializeField]
        public float flickThreshold = 0.5f;
        [SerializeField]
        public float relativeDistToCenter = 0.7f;
        [SerializeField]
        public float textPadding = 0.07f;
        [SerializeField]
        public Color centerButtonColor;
        [SerializeField]
        public Color centerButtonHighlightColor;
        [SerializeField]
        public Color flickButtonColor;
        [SerializeField]
        public Color textColor;
    }

    [Serializable]
    public class CharSetup 
    {
        [SerializeField]
        public string centerChar = "a";
        [SerializeField]
        public string upChar = "b";
        [SerializeField]
        public string downChar = "c";
        [SerializeField]
        public string leftChar = "d";
        [SerializeField]
        public string rightChar = "e";
    }

    // config
    public float? fontSize = null;
    public Config config;
    public CharSetup charSetup;


    // state
    private Vector2 mousePosStart;
    private FlickType currFlick;
    private bool pressed = false;
    Dictionary<FlickType, FlickButton> flickMap = new Dictionary<FlickType, FlickButton>();

    // refs
    public FlickLayout parentFlickLayout;

    void Update()
    {
        ResizeFlickButtons();
        UpdateFlick();
    }

    // unity event method
    public void PointerDown()
    {
        pressed = true;
        mousePosStart = Input.mousePosition;
    }

    // unity event method
    public void PointerUp()
    {
        pressed = false;
        parentFlickLayout.UpdateCharacter(GetCurrentChar());
        ResetFlicks();
    }

    public void Init()
    {
        flickMap.Clear();
        flickMap.Add(FlickType.Up, new FlickButton(transform.Find("FlickUp").gameObject, FlickType.Up));
        flickMap.Add(FlickType.Down, new FlickButton(transform.Find("FlickDown").gameObject, FlickType.Down));
        flickMap.Add(FlickType.Left, new FlickButton(transform.Find("FlickLeft").gameObject, FlickType.Left));
        flickMap.Add(FlickType.Right, new FlickButton(transform.Find("FlickRight").gameObject, FlickType.Right));
        flickMap.Add(FlickType.Center, new FlickButton(transform.Find("Center").gameObject, FlickType.Center));

        ResetFlicks();

        foreach (var flick in flickMap.Values)
        {
            // make sure all the game objects are active (prefab might be different)
            flick.SetActive(true);
            if (fontSize.HasValue) flick.SetFontSize(fontSize.Value);
        }

        SetColors();

        SetChars();
    }

    void ResizeFlickButtons()
    {
        foreach (var flick in flickMap.Values)
        {
            flick.Resize(config.relativeDistToCenter, config.textPadding);
        }
    }

    void ResetFlicks()
    {
        foreach (var flick in flickMap.Values)
        {
            flick.SetVisibility(false);
        }
        flickMap[FlickType.Center].SetVisibility(true);
        currFlick = FlickType.Center;
        SetColors();
    }

    void SetColors()
    {
        flickMap[FlickType.Up].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Down].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Left].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Right].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Center].SetColors(config.centerButtonColor, config.textColor);
    }

    void SetChars()
    {
        flickMap[FlickType.Up].character     = charSetup.upChar;
        flickMap[FlickType.Down].character   = charSetup.downChar;
        flickMap[FlickType.Left].character   = charSetup.leftChar;
        flickMap[FlickType.Right].character  = charSetup.rightChar;
        flickMap[FlickType.Center].character = charSetup.centerChar;
    }

    void UpdateFlick()
    {
        // figure out current flick type
        if (!pressed) return;
        UpdatePressed();
        Vector2 mousePosEnd = Input.mousePosition;
        Vector2 mouseDelta = mousePosEnd - mousePosStart;
        // center button
        if (mouseDelta.magnitude < config.flickThreshold) 
        {
            currFlick = FlickType.Center;
        }         
        // flick button
        else if (Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.y))
        {
            currFlick = mouseDelta.x > 0 ? FlickType.Right : FlickType.Left;
        }
        else
        {
            currFlick = mouseDelta.y > 0 ? FlickType.Up : FlickType.Down;
        }
        // center scenario
        if (currFlick == FlickType.Center)
        {
            ResetFlicks();
            return;
        }
        // flick scenario: set only the current flick/center visible
        else
        {
            foreach (var flickPair in flickMap)
            {
                flickPair.Value.SetVisibility(false);
            }
            flickMap[currFlick].SetVisibility(true);
            flickMap[FlickType.Center].SetVisibility(true);
        }

    }

    void UpdatePressed() 
    {
        flickMap[FlickType.Center].SetColors(config.centerButtonHighlightColor);
        transform.SetAsLastSibling();
    }

    private string GetCurrentChar()
    {
        string result = flickMap[currFlick].character;
        return result;
    }

}
