using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FlickInputHandler 
{
    public Keyboard keyboard;

    // char detection
    private PromptChar curPromptChar = null;
    private char? inputChar = null;
    private bool waitOne = false;

    public void SetPromptChar(PromptChar promptChar)
    {
        Reset();
        curPromptChar = promptChar;
        waitOne = RequiresModifier(promptChar.character);
    }

    private void Reset()
    {
        curPromptChar = null;
        inputChar = null;
        waitOne = false;
    }

    // to be called from the buttons
    public void UpdateCharacter(char newChar)
    {
        if (curPromptChar == null) return;

        Debug.Log("newChar: " + newChar + ", waitOne: " + waitOne);

        if (IsModifier(newChar) && inputChar.HasValue)
        {
            HandleModifier(newChar);
        }
        else
        {
            inputChar = newChar;
        }

        // wait if a modifier is required
        if (!waitOne)
        {
            if (inputChar.HasValue && inputChar.Value == curPromptChar.character)
            {
                //HACK: should use a delegate 
                keyboard.CharUpdatedSuccesfully();
            }
        }
        else
        {
            waitOne = false;
        }
    }

    private static bool IsModifier(char character)
    {
        string modifierChars = "小゛゜";
        return modifierChars.Contains(character) ? true : false;
    }

    private static bool RequiresModifier(char character)
    {
        switch (character)
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
            case 'ぱ':
            case 'ぽ':
            case 'ぴ':
            case 'ぷ':
            case 'ぺ':
                return true;
            default:
                return false;
        }
    }

    private void HandleModifier(char modChar)
    {
        if (inputChar == null) return;
        switch (modChar)
        {
            case '小':
                inputChar = ApplySmall(inputChar.Value);
                break;
            case '゛':
                inputChar = ApplyHandakuten(inputChar.Value);
                break;
            case '゜':
                inputChar = ApplyDakuten(inputChar.Value);
                break;
            default:
                break;
        }
    }

    private static char ApplySmall(char inputChar)
    {
        switch (inputChar)
        {
            case 'や': return 'ゃ';
            case 'ゆ': return 'ゅ';
            case 'よ': return 'ょ';
            case 'つ': return 'っ';
            default:
                return ' ';
        }
    }

    private static char ApplyHandakuten(char inputChar)
    {
        switch (inputChar)
        {
            case 'か': return 'が';
            case 'き': return 'ぎ';
            case 'く': return 'ぐ';
            case 'け': return 'げ';
            case 'こ': return 'ご';
            case 'さ': return 'ざ';
            case 'し': return 'じ';
            case 'す': return 'ず';
            case 'せ': return 'ぜ';
            case 'そ': return 'ぞ';
            case 'た': return 'だ';
            case 'ち': return 'ぢ';
            case 'つ': return 'づ';
            case 'て': return 'で';
            case 'と': return 'ど';
            case 'は': return 'ば';
            case 'ひ': return 'び';
            case 'ふ': return 'ぶ';
            case 'へ': return 'べ';
            case 'ほ': return 'ぼ';
            default:
                return ' ';
        }
    }

    private static char ApplyDakuten(char inputChar)
    {
        switch (inputChar)
        {
            case 'は': return 'ぱ';
            case 'ひ': return 'ぴ';
            case 'ふ': return 'ぷ';
            case 'へ': return 'ぺ';
            case 'ほ': return 'ぽ';
            default:
                return ' ';
        }
    }
}


/// <summary>
///  Hosts the flick buttons in responsive grid
/// </summary>
public class FlickLayout : MonoBehaviour
{
    // config
    private readonly int rows = 4;
    private readonly int columns = 3;
    private InputType type;
    [SerializeField]
    private FlickButton.Config buttonConfig;
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

    // flick button input
    public FlickInputHandler inputHandler = new FlickInputHandler();

    private void Awake()
    {
        flickKeyRectArr = new RectTransform[rows, columns];
        containerRect = transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
    }

    private void SetUpButton(FlickButton button, int r, int c)
    {
        switch (type)
        {
            case InputType.KeyHiragana:
                if (r == 0 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "小", upChar = "゛", downChar = "゜", leftChar = "　", rightChar = "　" }; }
                if (r == 0 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "わ", upChar = "ん", downChar = "　", leftChar = "を", rightChar = "ー" }; }
                if (r == 1 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ま", upChar = "む", downChar = "も", leftChar = "み", rightChar = "め" }; }
                if (r == 1 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "や", upChar = "ゆ", downChar = "よ", leftChar = "　", rightChar = "　" }; }
                if (r == 1 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ら", upChar = "る", downChar = "ろ", leftChar = "り", rightChar = "れ" }; }
                if (r == 2 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "た", upChar = "つ", downChar = "と", leftChar = "ち", rightChar = "て" }; }
                if (r == 2 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "な", upChar = "ぬ", downChar = "の", leftChar = "に", rightChar = "ね" }; }
                if (r == 2 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "は", upChar = "ふ", downChar = "ほ", leftChar = "ひ", rightChar = "へ" }; }
                if (r == 3 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "あ", upChar = "う", downChar = "お", leftChar = "い", rightChar = "え" }; }
                if (r == 3 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "か", upChar = "く", downChar = "こ", leftChar = "き", rightChar = "け" }; }
                if (r == 3 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "さ", upChar = "す", downChar = "そ", leftChar = "し", rightChar = "せ" }; }
                break;
            case InputType.KeyKatakana:
                if (r == 0 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "小", upChar = "゛", downChar = "゜", leftChar = "　", rightChar = "　" }; }
                if (r == 0 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ワ", upChar = "ン", downChar = "　", leftChar = "ヲ", rightChar = "ー" }; }
                if (r == 1 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "マ", upChar = "ム", downChar = "モ", leftChar = "ミ", rightChar = "メ" }; }
                if (r == 1 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ヤ", upChar = "ユ", downChar = "ヨ", leftChar = "　", rightChar = "　" }; }
                if (r == 1 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ラ", upChar = "ル", downChar = "ロ", leftChar = "リ", rightChar = "レ" }; }
                if (r == 2 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "タ", upChar = "ツ", downChar = "ト", leftChar = "チ", rightChar = "テ" }; }
                if (r == 2 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ナ", upChar = "ヌ", downChar = "ノ", leftChar = "ニ", rightChar = "ネ" }; }
                if (r == 2 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ハ", upChar = "フ", downChar = "ホ", leftChar = "ヒ", rightChar = "へ" }; }
                if (r == 3 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ア", upChar = "ウ", downChar = "オ", leftChar = "イ", rightChar = "エ" }; }
                if (r == 3 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "カ", upChar = "ク", downChar = "コ", leftChar = "キ", rightChar = "ケ" }; }
                if (r == 3 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "サ", upChar = "ス", downChar = "ソ", leftChar = "シ", rightChar = "セ" }; }
                break;
            case InputType.KeyRomaji:
                if (r == 0 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "小", upChar = "゛", downChar = "゜", leftChar = "　", rightChar = "　" }; }
                if (r == 0 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "wa", upChar = "n", downChar = "　", leftChar = "wo", rightChar = "ー" }; }
                if (r == 1 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ma", upChar = "mu", downChar = "mo", leftChar = "mi", rightChar = "me" }; }
                if (r == 1 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ya", upChar = "yu", downChar = "yo", leftChar = "　", rightChar = "　" }; }
                if (r == 1 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ra", upChar = "ru", downChar = "ro", leftChar = "ri", rightChar = "re" }; }
                if (r == 2 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ta", upChar = "tsu", downChar = "to", leftChar = "chi", rightChar = "te" }; }
                if (r == 2 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "na", upChar = "nu", downChar = "no", leftChar = "ni", rightChar = "ne" }; }
                if (r == 2 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ha", upChar = "fu", downChar = "ho", leftChar = "hi", rightChar = "he" }; }
                if (r == 3 && c == 0) { button.charSetup = new FlickButton.CharSetup() { centerChar = "a", upChar = "u", downChar = "o", leftChar = "ii", rightChar = "e" }; }
                if (r == 3 && c == 1) { button.charSetup = new FlickButton.CharSetup() { centerChar = "ka", upChar = "ku", downChar = "ko", leftChar = "ki", rightChar = "ke" }; }
                if (r == 3 && c == 2) { button.charSetup = new FlickButton.CharSetup() { centerChar = "sa", upChar = "su", downChar = "so", leftChar = "shi", rightChar = "se" }; }
                break;
        }
        if (overrideFontSize) 
        {
            button.fontSize = fontSize;
        }
    }

    void Update()
    {
        if(inputHandler.keyboard == null) inputHandler.keyboard = keyboard;       
        SetCellDimensions();
        UpdateGrid();
    }

    public void Init()
    {
        SetCellDimensions();
        CreateFlickButtons();
    }

    public void SetType(InputType type)
    {
        this.type = type;
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
                    FlickButton button = Instantiate(buttonPrefab, transform).GetComponent<FlickButton>();
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

    #region UI resizing 

    private void SetCellDimensions()
    {
        cellWidth = containerRect.rect.width / (float)(columns);
        cellHeight = containerRect.rect.height / (float)(rows);
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

    #endregion
}