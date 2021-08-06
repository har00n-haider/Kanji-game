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
        Center,
        None
    }

    private class FlickButton
    {
        public GameObject gameOject;
        public TextMeshProUGUI textMesh;
        public Image image;
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
            this.gameOject = gameOject;
            textMesh = gameOject.GetComponentInChildren<TextMeshProUGUI>();
            image = gameOject.GetComponent<Image>();

        }
        public void SetVisibility(bool value)
        {
            image.enabled = value;
            textMesh.enabled = value;
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
    private GameObject flicks;
    [SerializeField]
    private GameObject centerButton;
    [SerializeField]
    private ButtonConfig buttonConfig;

    private Vector2 mousePosStart;
    private FlickType currFlick;
    private bool pressed = false;


    private void Awake()
    {
        flickMap.Add(FlickType.Up, new FlickButton(flicks.transform.Find("FlickUp").gameObject));
        flickMap.Add(FlickType.Down, new FlickButton(flicks.transform.Find("FlickDown").gameObject));
        flickMap.Add(FlickType.Left, new FlickButton(flicks.transform.Find("FlickLeft").gameObject));
        flickMap.Add(FlickType.Right, new FlickButton(flicks.transform.Find("FlickRight").gameObject));
        flickMap.Add(FlickType.Center, new FlickButton(centerButton));

        ResetFlicks();
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
        ResetFlicks();
        Debug.Log(GetCurrentChar());

    }

    void ResetFlicks()
    {
        foreach (var flick in flickMap.Values)
        {
            flick.SetVisibility(false);
        }
        flickMap[FlickType.Center].SetVisibility(true);
        currFlick = FlickType.None;
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
        if (currFlick == FlickType.None)
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
        if(currFlick != FlickType.None) 
        {
            result =  flickMap[currFlick].character;
        }
        return result;
    }

}
