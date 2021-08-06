using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

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

    private class FlickButton
    {
        private RectTransform buttonRect;
        private RectTransform imageRect;
        public GameObject gameObject; // top level game object
        private TextMeshProUGUI textMesh;
        private Image image;
        private Vector2 initalPosScale;
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
        public FlickButton(GameObject gameOject)
        {
            this.gameObject = gameOject;
            textMesh = gameOject.GetComponentInChildren<TextMeshProUGUI>();
            image = gameOject.GetComponent<Image>();
            imageRect = gameOject.GetComponent<RectTransform>();
            buttonRect = gameOject.GetComponentInParent<RectTransform>();

            Debug.Log(buttonRect.rect.width + " " + buttonRect.rect.width);
            Debug.Log(imageRect.anchoredPosition);
            initalPosScale.x = buttonRect.rect.width / imageRect.anchoredPosition.x;
            initalPosScale.y = buttonRect.rect.height / imageRect.anchoredPosition.y;
        }
        public void SetVisibility(bool value, bool exceptText = false)
        {
            image.enabled = value;
            textMesh.enabled = value;
        }
        // assumes that the flick buttons are in stretch mode
        public void Resize()
        {
            // old
            float buttonHeight = buttonRect.rect.height;
            float buttonWidth = buttonRect.rect.width;
            Vector2 newPos = new Vector2(
               buttonWidth / initalPosScale.x,
               buttonHeight / initalPosScale.y
            );
            imageRect.anchoredPosition = newPos;
            textMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonHeight / 3);
            textMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonWidth / 3);
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
        flickMap.Add(FlickType.Up, new FlickButton(transform.Find("FlickUp").gameObject));
        flickMap.Add(FlickType.Down, new FlickButton(transform.Find("FlickDown").gameObject));
        flickMap.Add(FlickType.Left, new FlickButton(transform.Find("FlickLeft").gameObject));
        flickMap.Add(FlickType.Right, new FlickButton(transform.Find("FlickRight").gameObject));
        flickMap.Add(FlickType.Center, new FlickButton(transform.Find("Center").gameObject));

        ResetFlicks();

        foreach (var flick in flickMap.Values)
        {
            flick.gameObject.SetActive(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        flickMap[FlickType.Up].character = buttonConfig.upChar;
        flickMap[FlickType.Down].character = buttonConfig.downChar;
        flickMap[FlickType.Left].character = buttonConfig.leftChar;
        flickMap[FlickType.Right].character = buttonConfig.rightChar;
        flickMap[FlickType.Center].character = buttonConfig.centerChar;
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

    void ResizeFlickButtons()
    {
        foreach (var flick in flickMap.Values)
        {
            flick.Resize();
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
    }

    void UpdateFlick()
    {
        // figure out current flick type
        if (!pressed) return;
        Vector2 mousePosEnd = Input.mousePosition;
        Vector2 mouseDelta = mousePosEnd - mousePosStart;
        if (mouseDelta.magnitude < buttonConfig.flickThreshold) return;
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
        // set only the current flick visible
        else
        {
            foreach (var flickPair in flickMap)
            {
                flickPair.Value.SetVisibility(false);
            }
            flickMap[currFlick].SetVisibility(true);
        }

    }

    char? GetCurrentChar()
    {
        char? result = null;
        result = flickMap[currFlick].character;
        return result;
    }

}
