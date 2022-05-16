using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class UIUtils
{
    /// <summary>
    ///  Get the Rect that encapsulates the rect transform in world space
    /// </summary>
    /// <param name="transform"></param>
    /// <returns></returns>
    public static Rect RectTransformToWorldRect(RectTransform transform)
    {
        Vector3[] WorldCorners = new Vector3[4];
        transform.GetWorldCorners(WorldCorners);
        Bounds bounds = new Bounds(WorldCorners[0], Vector3.zero);
        for (int i = 1; i < 4; ++i)
        {
            bounds.Encapsulate(WorldCorners[i]);
        }
        Rect screenRect = new Rect(bounds.min, bounds.size);
        return screenRect;
    }

    public static void StretchToParentSize(this RectTransform rect, RectTransform parent, Vector2? pivot = null)
    {
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        rect.pivot = !pivot.HasValue ? new Vector2(0.5f, 0.5f) : pivot.Value;
    }

    /// <summary>
    /// For when the position of a given rect needs to stay constant relative
    /// to the parent, when its size is changing
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="parent"></param>
    /// <param name="scale"></param>
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

    public static void SetLocalPositionAndDims(this RectTransform rect, Vector2 position, float height, float width)
    {
        rect.anchoredPosition = position;
        rect.pivot = new Vector2();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public static string AddColor(this string text, Color col) => $"<color={ColorHexFromUnityColor(col)}>{text}</color>";

    public static string ColorHexFromUnityColor(this Color unityColor) => $"#{ColorUtility.ToHtmlStringRGBA(unityColor)}";

    // Aligns a transform on the canvas with an object in the scene
    public static void UpdateLabelScreenPos(RectTransform lRect, float yOffsetPercentage, Vector3 targetWorldPos)
    {
        if (lRect == null) return;
        float labelRectW = lRect.rect.width;
        float labelRectH = lRect.rect.height;
        float sH = Camera.main.pixelHeight;
        float sW = Camera.main.pixelWidth;
        // update the location of the label on the screen
        var screenpoint = Camera.main.WorldToScreenPoint(targetWorldPos);
        // smooth clamp rect to the screen dimensions
        float labelY = GeometryUtils.ClampLengthToRegion(screenpoint.y, labelRectH, sH);
        float labelX = GeometryUtils.ClampLengthToRegion(screenpoint.x, labelRectW, sW);
        // apply vertical offset to label
        float labelYOffset = yOffsetPercentage * sH;
        lRect.position = new Vector2(labelX, labelY + labelYOffset);
    }
}