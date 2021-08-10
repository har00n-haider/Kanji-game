using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    private int rows = 4;
    private int columns = 3;
    private RectTransform gridRect;

    private float cellWidth;
    private float cellHeight;
    private RectTransform[,] grid;

    [SerializeField]
    private GameObject buttonPrefab;
    [SerializeField]
    private GameObject buttonPlaceholder;


    [SerializeField]
    private Button.Config buttonConfig;

    private void Awake()
    {
        grid = new RectTransform[rows, columns];
        gridRect = GetComponent<RectTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetCellDimensions();

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
                    Button button = Instantiate(buttonPrefab, transform).GetComponent<Button>();
                    button.name = "button " + buttNo;
                    SetUpButton(button, r, c);
                    button.config = buttonConfig;
                    button.Init();
                    currCell = button.gameObject;
                }
                // get ref to ret for fitting
                RectTransform cellRect = currCell.GetComponent<RectTransform>();
                SetGridPosForCell(cellRect, gridRect, r, c);
                grid[r, c] = cellRect;
                buttNo++;
            }
        }
    }

    void SetUpButton(Button button, int r, int c)
    {
        if (r == 0 && c == 1) { button.charSetup = new Button.CharSetup() { centerChar = 'わ', upChar = 'ん', downChar = '　', leftChar = 'を', rightChar = 'ー' }; }
        if (r == 1 && c == 0) { button.charSetup = new Button.CharSetup() { centerChar = 'ま', upChar = 'む', downChar = 'も', leftChar = 'み', rightChar = 'め' }; }
        if (r == 1 && c == 1) { button.charSetup = new Button.CharSetup() { centerChar = 'や', upChar = 'ゆ', downChar = 'よ', leftChar = '　', rightChar = '　' }; }
        if (r == 1 && c == 2) { button.charSetup = new Button.CharSetup() { centerChar = 'ら', upChar = 'る', downChar = 'ろ', leftChar = 'り', rightChar = 'れ' }; }
        if (r == 2 && c == 0) { button.charSetup = new Button.CharSetup() { centerChar = 'た', upChar = 'つ', downChar = 'と', leftChar = 'ち', rightChar = 'て' }; }
        if (r == 2 && c == 1) { button.charSetup = new Button.CharSetup() { centerChar = 'な', upChar = 'ぬ', downChar = 'の', leftChar = 'に', rightChar = 'ね' }; }
        if (r == 2 && c == 2) { button.charSetup = new Button.CharSetup() { centerChar = 'は', upChar = 'ふ', downChar = 'ほ', leftChar = 'ひ', rightChar = 'へ' }; }
        if (r == 3 && c == 0) { button.charSetup = new Button.CharSetup() { centerChar = 'あ', upChar = 'う', downChar = 'お', leftChar = 'い', rightChar = 'え' }; }
        if (r == 3 && c == 1) { button.charSetup = new Button.CharSetup() { centerChar = 'か', upChar = 'く', downChar = 'こ', leftChar = 'き', rightChar = 'け' }; }
        if (r == 3 && c == 2) { button.charSetup = new Button.CharSetup() { centerChar = 'さ', upChar = 'す', downChar = 'そ', leftChar = 'し', rightChar = 'せ' }; }
    }

    void Update()
    {
        SetCellDimensions();
        UpdateGrid();
    }

    private void SetCellDimensions()
    {
        cellWidth = gridRect.sizeDelta.x / (float)(columns);
        cellHeight = gridRect.sizeDelta.y / (float)(rows);
    }

    private void UpdateGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                SetGridPosForCell(grid[r, c], gridRect, r, c);
                SetGridCellSizeForCell(grid[r, c], cellHeight, cellWidth);
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


