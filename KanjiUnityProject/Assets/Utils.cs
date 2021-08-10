using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class Utils
{

    public static void SetAndStretchToParentSize(this RectTransform rect, RectTransform parent)
    {
        rect.anchoredPosition = parent.position;
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = parent.rect.size;
    }

    public static void ScalePosRelativeToParentSize(this RectTransform rect, RectTransform parent, Vector2 scale)
    {
        float buttonHeight = parent.rect.height;
        float buttonWidth = parent.rect.width;
        Vector2 newPos = new Vector2(
            buttonWidth * scale.x,
            buttonHeight * scale.y
        );
        rect.anchoredPosition = newPos;
    }

    // Values should be normalized
    public static void SetPadding(this RectTransform rect, float up, float down, float left, float right)
    {
        rect.anchoredPosition = new Vector2();
        rect.anchorMin = new Vector2(left, down);
        rect.anchorMax = new Vector2(1 - right, 1 - up);
        rect.pivot = new Vector2(0.5f, 0.5f);
        // the size of the rect is adjusted to match the
        // rect formed by the location of the anchor points
        rect.sizeDelta = new Vector2();
    }



}

