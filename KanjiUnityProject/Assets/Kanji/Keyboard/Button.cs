using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Button : MonoBehaviour
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

        public char character
        {
            get
            {
                char val = ' ';
                val = textMesh.text.Length > 0 ? textMesh.text[0] : val;
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
            image = flickButtonGameObject.GetComponent<Image>();
            imageRect = flickButtonGameObject.GetComponent<RectTransform>();
            buttonRect = flickButtonGameObject.GetComponentInParent<RectTransform>();
            relativePosScale.x = buttonRect.rect.width / imageRect.anchoredPosition.x;
            relativePosScale.y = buttonRect.rect.height / imageRect.anchoredPosition.y;
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
    private class ButtonConfig
    {
        [SerializeField]
        public char centerChar;
        [SerializeField]
        public char upChar;
        [SerializeField]
        public char downChar;
        [SerializeField]
        public char leftChar;
        [SerializeField]
        public char rightChar;
        [SerializeField]
        public float flickThreshold;
        [SerializeField]
        public float relativeDistToCenter;
        [SerializeField]
        public float padding;
        [SerializeField]
        public Color centerButtonColor;
        [SerializeField]
        public Color centerButtonHighlightColor;
        [SerializeField]
        public Color flickButtonColor;
        [SerializeField]
        public Color textColor;

    }

    Dictionary<FlickType, FlickButton> flickMap = new Dictionary<FlickType, FlickButton>();

    [SerializeField]
    private GameObject centerButton;
    [SerializeField]
    private ButtonConfig buttonConfig;

    private Vector2 mousePosStart;
    private FlickType currFlick;
    private bool pressed = false;


    private void Awake()
    {
        flickMap.Add(FlickType.Up, new FlickButton(transform.Find("FlickUp").gameObject, FlickType.Up));
        flickMap.Add(FlickType.Down, new FlickButton(transform.Find("FlickDown").gameObject, FlickType.Down));
        flickMap.Add(FlickType.Left, new FlickButton(transform.Find("FlickLeft").gameObject, FlickType.Left));
        flickMap.Add(FlickType.Right, new FlickButton(transform.Find("FlickRight").gameObject, FlickType.Right));
        flickMap.Add(FlickType.Center, new FlickButton(transform.Find("Center").gameObject, FlickType.Center));

        ResetFlicks();

        foreach (var flick in flickMap.Values)
        {
            flick.SetActive(true);
        }

        SetColors();

        SetChars();

    }

    // Start is called before the first frame update
    void Start()
    {

    }


    // Update is called once per frame
    void Update()
    {
        ResizeFlickButtons();
        UpdateFlick();
    }

    public void PointerDown()
    {
        pressed = true;
        mousePosStart = Input.mousePosition;
    }

    public void PointerUp()
    {
        pressed = false;
        Debug.Log(GetCurrentChar());
        ResetFlicks();
    }

    void SetColors() 
    {
        flickMap[FlickType.Up].SetColors(buttonConfig.flickButtonColor,   buttonConfig.textColor);
        flickMap[FlickType.Down].SetColors(buttonConfig.flickButtonColor, buttonConfig.textColor);
        flickMap[FlickType.Left].SetColors(buttonConfig.flickButtonColor, buttonConfig.textColor);
        flickMap[FlickType.Right].SetColors(buttonConfig.flickButtonColor, buttonConfig.textColor);
        flickMap[FlickType.Center].SetColors(buttonConfig.centerButtonColor, buttonConfig.textColor);
    }

    void SetChars() 
    {
        flickMap[FlickType.Up].character = buttonConfig.upChar;
        flickMap[FlickType.Down].character = buttonConfig.downChar;
        flickMap[FlickType.Left].character = buttonConfig.leftChar;
        flickMap[FlickType.Right].character = buttonConfig.rightChar;
        flickMap[FlickType.Center].character = buttonConfig.centerChar;
    }

    void ResizeFlickButtons()
    {
        foreach (var flick in flickMap.Values)
        {
            flick.Resize(buttonConfig.relativeDistToCenter, buttonConfig.padding);
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

    void UpdateFlick()
    {
        // figure out current flick type
        if (!pressed) return;
        flickMap[FlickType.Center].SetColors(buttonConfig.centerButtonHighlightColor);
        Vector2 mousePosEnd = Input.mousePosition;
        Vector2 mouseDelta = mousePosEnd - mousePosStart;
        if (mouseDelta.magnitude < buttonConfig.flickThreshold) return;
        // flick occured
        if (Mathf.Abs(mouseDelta.x) > Mathf.Abs(mouseDelta.y))
        {
            currFlick = mouseDelta.x > 0 ? FlickType.Right : FlickType.Left;
        }
        else
        {
            currFlick = mouseDelta.y > 0 ? FlickType.Up : FlickType.Down;
        }
        // nothing happened scenario
        if (currFlick == FlickType.Center)
        {
            ResetFlicks();
            return;
        }
        // set only the current flick/center visible
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

    char? GetCurrentChar()
    {
        char? result = null;
        result = flickMap[currFlick].character;
        return result;
    }

}
