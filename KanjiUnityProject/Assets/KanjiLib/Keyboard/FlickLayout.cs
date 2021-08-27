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
    [SerializeField]
    private CharType type;
    [SerializeField]
    private float fontSize;
    [SerializeField]
    private bool overrideFontSize = false;

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
    [HideInInspector]
    public Keyboard keyboard;

    // mora detection
    private PromptChar currCharTarget = null;
    private string inputStr = "";
    private bool waitOne = false;


    private void Awake()
    {
        flickKeyRectArr = new RectTransform[rows, columns];
        containerRect = transform.parent.GetComponent<RectTransform>();
    }

    void SetUpButton(KeyboardButton button, int r, int c)
    {
        switch (type)
        {
            case CharType.Hiragana:
                if (r == 0 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "小", upChar = "゛", downChar = "゜", leftChar = "　", rightChar = "　" }; }
                if (r == 0 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "わ", upChar = "ん", downChar = "　", leftChar = "を", rightChar = "ー" }; }
                if (r == 1 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ま", upChar = "む", downChar = "も", leftChar = "み", rightChar = "め" }; }
                if (r == 1 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "や", upChar = "ゆ", downChar = "よ", leftChar = "　", rightChar = "　" }; }
                if (r == 1 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ら", upChar = "る", downChar = "ろ", leftChar = "り", rightChar = "れ" }; }
                if (r == 2 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "た", upChar = "つ", downChar = "と", leftChar = "ち", rightChar = "て" }; }
                if (r == 2 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "な", upChar = "ぬ", downChar = "の", leftChar = "に", rightChar = "ね" }; }
                if (r == 2 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "は", upChar = "ふ", downChar = "ほ", leftChar = "ひ", rightChar = "へ" }; }
                if (r == 3 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "あ", upChar = "う", downChar = "お", leftChar = "い", rightChar = "え" }; }
                if (r == 3 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "か", upChar = "く", downChar = "こ", leftChar = "き", rightChar = "け" }; }
                if (r == 3 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "さ", upChar = "す", downChar = "そ", leftChar = "し", rightChar = "せ" }; }
                break;
            case CharType.Katana:
                if (r == 0 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "小", upChar = "゛", downChar = "゜", leftChar = "　", rightChar = "　" }; }
                if (r == 0 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ワ", upChar = "ン", downChar = "　", leftChar = "ヲ", rightChar = "ー" }; }
                if (r == 1 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "マ", upChar = "ム", downChar = "モ", leftChar = "ミ", rightChar = "メ" }; }
                if (r == 1 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ヤ", upChar = "ユ", downChar = "ヨ", leftChar = "　", rightChar = "　" }; }
                if (r == 1 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ラ", upChar = "ル", downChar = "ロ", leftChar = "リ", rightChar = "レ" }; }
                if (r == 2 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "タ", upChar = "ツ", downChar = "ト", leftChar = "チ", rightChar = "テ" }; }
                if (r == 2 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ナ", upChar = "ヌ", downChar = "ノ", leftChar = "ニ", rightChar = "ネ" }; }
                if (r == 2 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ハ", upChar = "フ", downChar = "ホ", leftChar = "ヒ", rightChar = "へ" }; }
                if (r == 3 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ア", upChar = "ウ", downChar = "オ", leftChar = "イ", rightChar = "エ" }; }
                if (r == 3 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "カ", upChar = "ク", downChar = "コ", leftChar = "キ", rightChar = "ケ" }; }
                if (r == 3 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "サ", upChar = "ス", downChar = "ソ", leftChar = "シ", rightChar = "セ" }; }
                break;
            case CharType.Romaji:
                if (r == 0 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "小", upChar = "゛", downChar = "゜", leftChar = "　", rightChar = "　" }; }
                if (r == 0 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "wa", upChar = "n", downChar = "　", leftChar = "wo", rightChar = "ー" }; }
                if (r == 1 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ma", upChar = "mu", downChar = "mo", leftChar = "mi", rightChar = "me" }; }
                if (r == 1 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ya", upChar = "yu", downChar = "yo", leftChar = "　", rightChar = "　" }; }
                if (r == 1 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ra", upChar = "ru", downChar = "ro", leftChar = "ri", rightChar = "re" }; }
                if (r == 2 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ta", upChar = "tsu", downChar = "to", leftChar = "chi", rightChar = "te" }; }
                if (r == 2 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "na", upChar = "nu", downChar = "no", leftChar = "ni", rightChar = "ne" }; }
                if (r == 2 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ha", upChar = "fu", downChar = "ho", leftChar = "hi", rightChar = "he" }; }
                if (r == 3 && c == 0) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "a", upChar = "u", downChar = "o", leftChar = "ii", rightChar = "e" }; }
                if (r == 3 && c == 1) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "ka", upChar = "ku", downChar = "ko", leftChar = "ki", rightChar = "ke" }; }
                if (r == 3 && c == 2) { button.charSetup = new KeyboardButton.CharSetup() { centerChar = "sa", upChar = "su", downChar = "so", leftChar = "shi", rightChar = "se" }; }
                break;
        }
        if (overrideFontSize) 
        {
            button.fontSize = fontSize;
        }
    }

    void Update()
    {
        SetCellDimensions();
        UpdateGrid();
    }

    public void Init()
    {
        SetCellDimensions();
        CreateFlickButtons();
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
                    // make button
                    KeyboardButton button = Instantiate(buttonPrefab, transform).GetComponent<KeyboardButton>();
                    button.name = "button " + buttNo;
                    SetUpButton(button, r, c);
                    button.config = buttonConfig;
                    button.parentFlickLayout = this;
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

    public void SetPromptChar(PromptChar promptChar) 
    {
        currCharTarget = promptChar;
        waitOne = RequiresModifier(promptChar.character);
    }

    // to be called from the buttons
    public void UpdateCharacter(string inputchar) 
    {
        HandleInputChar(inputchar);
        if (!waitOne) 
        {
            if(inputStr == currCharTarget.character) 
            {
                keyboard.CharCompletedSuccesfully();
            }
            else 
            {
                waitOne = RequiresModifier(currCharTarget.character);
            }
        }
        else 
        {
            waitOne = false;
        }
    }

    private bool RequiresModifier(string character) 
    {
        switch (character[0]) 
        {
            // smalleable
            case 'ゃ':
            case 'ゅ':
            case 'ょ':
            case 'っ':
            // han/dakuten
            case 'が':
            case 'ぎ':
            case 'ぐ':
            case 'げ':
            case 'ご':
            case 'ざ':
            case 'じ':
            case 'ず':
            case 'ぜ':
            case 'ぞ':
            case 'だ':
            case 'ぢ':
            case 'づ':
            case 'で':
            case 'ど':
            case 'ば':
            case 'び':
            case 'ぶ':
            case 'べ':
            case 'ぼ':
            case 'ぽ':
            case 'ぴ':
            case 'ぷ':
            case 'ぺ':
                return true;
            default:
                return false;
        }
    }

    private void HandleInputChar(string inputChar) 
    {
        switch (inputChar)
        {
            case "小":
                ApplySmall();
                break;
            case "゛":
                ApplyHandakuten();
                break;
            case "゜":
                ApplyDakuten();
                break;
            default:
                inputStr = inputChar; 
                break;
        }
    }

    private void ApplySmall() 
    {
        switch (inputStr) 
        {
            case "や": inputStr = "ゃ"; break;
            case "ゆ": inputStr = "ゅ"; break;
            case "よ": inputStr = "ょ"; break;
            case "つ": inputStr = "っ"; break;
        }
    }

    private void ApplyHandakuten() 
    {
        switch (inputStr) 
        {
            case "か": inputStr = "が"; break;
            case "き": inputStr = "ぎ"; break;
            case "く": inputStr = "ぐ"; break;
            case "け": inputStr = "げ"; break;
            case "こ": inputStr = "ご"; break;
            case "さ": inputStr = "ざ"; break;
            case "し": inputStr = "じ"; break;
            case "す": inputStr = "ず"; break;
            case "せ": inputStr = "ぜ"; break;
            case "そ": inputStr = "ぞ"; break;
            case "た": inputStr = "だ"; break;
            case "ち": inputStr = "ぢ"; break;
            case "つ": inputStr = "づ"; break;
            case "て": inputStr = "で"; break;
            case "と": inputStr = "ど"; break;
            case "は": inputStr = "ば"; break;
            case "ひ": inputStr = "び"; break;
            case "ふ": inputStr = "ぶ"; break;
            case "へ": inputStr = "べ"; break;
            case "ほ": inputStr = "ぼ"; break;
        }
    }

    private void ApplyDakuten()
    {
        switch (inputStr)
        {
            case "は": inputStr = "ぽ"; break;
            case "ひ": inputStr = "ぴ"; break;
            case "ふ": inputStr = "ぷ"; break;
            case "へ": inputStr = "ぺ"; break;
            case "ほ": inputStr = "ぽ"; break;
        }
    }
}