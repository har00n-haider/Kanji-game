using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and manages the flick buttons for a given button in the keyboard.
/// Deals primarily with the interaction logic (UI resizing is done 
/// by the flick button itself)
/// </summary>
public class FlickButton : MonoBehaviour
{
    [Serializable]
    public class Config
    {
        [SerializeField]
        public float flickThreshold = 30;
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
        [SerializeField]
        public bool disabled = false;
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
    Dictionary<FlickType, FlickButtonDirectional> flickMap = new Dictionary<FlickType, FlickButtonDirectional>();

    // refs
    public FlickLayout parentFlickLayout;

    void Update()
    {
        ResizeFlickButtons();
        if (!config.disabled) UpdateFlick();
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
        // TODO: Use delegate
        parentFlickLayout.UpdateFromInput(GetCurrentChar());
        ResetFlicks();
        SetColors();
    }

    public void Init()
    {
        flickMap.Clear();
        flickMap.Add(FlickType.Up, new FlickButtonDirectional(transform.Find("FlickUp").gameObject, FlickType.Up));
        flickMap.Add(FlickType.Down, new FlickButtonDirectional(transform.Find("FlickDown").gameObject, FlickType.Down));
        flickMap.Add(FlickType.Left, new FlickButtonDirectional(transform.Find("FlickLeft").gameObject, FlickType.Left));
        flickMap.Add(FlickType.Right, new FlickButtonDirectional(transform.Find("FlickRight").gameObject, FlickType.Right));
        flickMap.Add(FlickType.Center, new FlickButtonDirectional(transform.Find("Center").gameObject, FlickType.Center));

        ResetFlicks();

        foreach (var flick in flickMap.Values)
        {
            // make sure all the game objects are active (prefab might be different)
            flick.SetActive(true);
            if (fontSize.HasValue) flick.SetFontSize(fontSize.Value);
        }

        SetColors();

        UpdateChars();
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
    }

    void SetColors()
    {
        flickMap[FlickType.Up].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Down].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Left].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Right].SetColors(config.flickButtonColor, config.textColor);
        flickMap[FlickType.Center].SetColors(config.centerButtonColor, config.textColor);
    }

    public void UpdateChars()
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
        
        // figure out the current flick type
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

        // act on the current flick type
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

    private char GetCurrentChar()
    {
        //TODO: Fix me 
        return flickMap[currFlick].character[0];
    }
}
