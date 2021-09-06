using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardUILayout : MonoBehaviour
{
    // refs
    public RectTransform displayRect;
    public RectTransform inputRect;
    private RectTransform parentRect;

    // config
    public float dispHToParentHRatio = 0.03f;

    // state
    private float inputHeight;
    private float displayHeight;

    void Start()
    {
        parentRect = GetComponentInParent<RectTransform>();        
    }

    void Update()
    {
        displayHeight = dispHToParentHRatio * parentRect.rect.height;
        inputHeight = (1 - dispHToParentHRatio) * parentRect.rect.height;

        inputRect.SetLocalPositionAndDims(
            new Vector2(0, 0),
            inputHeight,
            parentRect.rect.width
        );

        displayRect.SetLocalPositionAndDims(
            new Vector2(0, inputRect.rect.height),
            displayHeight,
            parentRect.rect.width
        );
    }
  
}
