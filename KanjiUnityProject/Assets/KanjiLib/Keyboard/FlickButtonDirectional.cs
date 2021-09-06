using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum FlickType
{
    Up,
    Down,
    Left,
    Right,
    Center
}


// currently designed to match a hierarchy of
// - KeyboardButton (top level game object)
//  - KeyboardFlickButton (with image, i.e. instance of this class)
//   - TexMesh 
public class FlickButtonDirectional
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

    public FlickButtonDirectional(GameObject flickButtonGameObject, FlickType type)
    {
        this.type = type;
        this.gameObject = flickButtonGameObject;
        textMesh = flickButtonGameObject.GetComponentInChildren<TextMeshProUGUI>();
        textMesh.font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Regular SDF");
        image = flickButtonGameObject.GetComponent<Image>();
        imageRect = flickButtonGameObject.GetComponent<RectTransform>();
        buttonRect = flickButtonGameObject.GetComponentInParent<RectTransform>();
        // the initial flick button position that you want to
        // keep constant as the parent scale (button rect) changes
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
            textMesh.color = textColor.Value;
        }
    }

    public void SetActive(bool state)
    {
        gameObject.SetActive(state);
    }

    #region UI resizing 

    // the actual resizing of the flick button is done via the UI
    // flick button is set to stretch
    public void Resize(float distFromCenter, float padding)
    {
        SetScaledRelativePosition(distFromCenter);
        imageRect.ScalePosRelativeToParentSize(buttonRect, relativePosScale);
        textMesh.rectTransform.SetPadding(padding, padding, padding, padding);
    }

    private void SetScaledRelativePosition(float distance)
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

    #endregion
}

