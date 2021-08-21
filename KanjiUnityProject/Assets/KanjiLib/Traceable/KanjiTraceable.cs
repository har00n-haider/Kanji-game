using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class KanjiTraceable : MonoBehaviour
{
    public bool selected { get; set; }

    // label config
    public Color labelColor;
    public Color textColor;
    public float labelOffsetYPercentage;


    private bool setColliderSize = false;

    // refs
    private RectTransform labelRect;
    public GameObject labelPrefab;
    private KanjiManager kanjiMan;

    private void Awake()
    {
        kanjiMan = GameObject.FindGameObjectWithTag("KanjiManager").GetComponent<KanjiManager>();
        kanjiMan.RegisterKanjiTraceable(this);

        // always place the label on the main canvas
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        GameObject label =  Instantiate(labelPrefab, mainCanvas.transform);
        labelRect = label.GetComponentInChildren<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateLabelScreenPos(labelRect, labelOffsetYPercentage);
    }

    // Update is called once per frame
    void Update()
    {
        // the label takes a frame or so to get the right size (mayebe the content fitter?)
        if (!setColliderSize && labelRect.rect.size.magnitude > 0) 
        {
            ConfigureLabel(labelRect.gameObject);
            setColliderSize = true;
        }
        UpdateLabelScreenPos(labelRect, labelOffsetYPercentage);
    }

    public bool IsDestroyed()
    {
        return gameObject == null;
    }

    private void ConfigureLabel(GameObject label) 
    {
        var labelText = label.GetComponentInChildren<TextMeshProUGUI>();
        labelText.color = textColor;
        var labelImage = label.GetComponentInChildren<Image>();
        labelImage.color = labelColor;
        var labelBoxCollider = label.GetComponent<BoxCollider2D>();
        labelBoxCollider.size = labelRect.rect.size;


    }

    private void UpdateLabelScreenPos(RectTransform lRect, float yOffsetPercentage)
    {
        float labelRectW = lRect.rect.width;
        float labelRectH = lRect.rect.height;
        float sH = Camera.main.pixelHeight;
        float sW = Camera.main.pixelWidth;
        // update the location of the label on the screen
        var screenpoint = Camera.main.WorldToScreenPoint(transform.position);
        // smooth clamp rect to the screen dimensions
        float labelY = GeometryUtils.ClampLengthToRegion(screenpoint.y, labelRectH, sH);
        float labelX = GeometryUtils.ClampLengthToRegion(screenpoint.x, labelRectW, sW);
        // apply vertical offset to label
        float labelYOffset = yOffsetPercentage * sH;
        lRect.position = new Vector2(labelX, labelY + labelYOffset);
    }

}