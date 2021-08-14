using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FlickLayout : MonoBehaviour
{

    // config
    private readonly int rows = 4;
    private readonly int columns = 3;
    [SerializeField]
    private KeyboardButton.Config buttonConfig;

    // flick key setup
    private float cellWidth;
    private float cellHeight;
    private RectTransform[,] flickKeyRectArr;
    private RectTransform containerRect;

    // refs
    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject buttonPlaceholder;


    private void Awake()
    {
        flickKeyRectArr = new RectTransform[rows, columns];
        containerRect = transform.parent.GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetCellDimensions();
        CreateFlickButtons();
    }

    void SetUpButton(KeyboardButton button, int r, int c)
    {
        if (r == 0 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'わ', upChar = 'ん', downChar = '　', leftChar = 'を', rightChar = 'ー' }; }
        if (r == 1 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'ま', upChar = 'む', downChar = 'も', leftChar = 'み', rightChar = 'め' }; }
        if (r == 1 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'や', upChar = 'ゆ', downChar = 'よ', leftChar = '　', rightChar = '　' }; }
        if (r == 1 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'ら', upChar = 'る', downChar = 'ろ', leftChar = 'り', rightChar = 'れ' }; }
        if (r == 2 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'た', upChar = 'つ', downChar = 'と', leftChar = 'ち', rightChar = 'て' }; }
        if (r == 2 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'な', upChar = 'ぬ', downChar = 'の', leftChar = 'に', rightChar = 'ね' }; }
        if (r == 2 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'は', upChar = 'ふ', downChar = 'ほ', leftChar = 'ひ', rightChar = 'へ' }; }
        if (r == 3 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'あ', upChar = 'う', downChar = 'お', leftChar = 'い', rightChar = 'え' }; }
        if (r == 3 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'か', upChar = 'く', downChar = 'こ', leftChar = 'き', rightChar = 'け' }; }
        if (r == 3 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = 'さ', upChar = 'す', downChar = 'そ', leftChar = 'し', rightChar = 'せ' }; }
    }

    void Update()
    {
        SetCellDimensions();
        UpdateGrid();
    }

    private void CreateFlickButtons()
    {
        int buttNo = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                // buttons we are skipping for the moment
                bool isPlaceholderButton =
                    r == 0 && c == 0 ||
                    r == 0 && c == 2;
                GameObject currCell = null;
                if (isPlaceholderButton)
                {
                    // make placeholder
                    currCell = Instantiate(buttonPlaceholder, transform);
                    currCell.GetComponent<Image>().color = buttonConfig.centerButtonColor;
                    currCell.name = "button " + buttNo;
                }
                else
                {
                    // make buttont
                    KeyboardButton button = Instantiate(buttonPrefab, transform).GetComponent<KeyboardButton>();
                    button.name = "button " + buttNo;
                    SetUpButton(button, r, c);
                    button.config = buttonConfig;
                    button.Init();
                    currCell = button.gameObject;
                }
                // get ref to ret for fitting
                RectTransform cellRect = currCell.GetComponent<RectTransform>();
                SetGridPosForCell(cellRect, containerRect, r, c);
                flickKeyRectArr[r, c] = cellRect;
                buttNo++;
            }
        }
    }

    private void SetCellDimensions()
    {
        cellWidth = containerRect.sizeDelta.x / (float)(columns);
        cellHeight = containerRect.sizeDelta.y / (float)(rows);
    }

    private void UpdateGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                SetGridPosForCell(flickKeyRectArr[r, c], containerRect, r, c);
                SetGridCellSizeForCell(flickKeyRectArr[r, c], cellHeight, cellWidth);
            }
        }
    }

    private void SetGridPosForCell(RectTransform rect, RectTransform parent, int r, int c)
    {
        // set cell origin at bottom left corner of parent
        rect.anchorMin = new Vector2();
        rect.anchorMax = new Vector2();
        rect.anchoredPosition = new Vector2(
            c * cellWidth + cellWidth / 2,
            r * cellHeight + cellHeight / 2);
    }

    private static void SetGridCellSizeForCell(RectTransform rect, float height, float width)
    {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }


}
