using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the input logic to create the relevant
/// output (e.g. when using modifier like 小゛゜)
/// </summary>
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
    private class Cell
    {
        public RectTransform tranform;
        public FlickButton button;
    }

    // config
    private readonly int rows = 4;
    private readonly int columns = 3;
    private InputType type;
    [SerializeField]
    private FlickButton.Config defaultButtonConfig;
    [SerializeField]
    private float fontSize;
    [SerializeField]
    private bool overrideFontSize = false;

    // flick key setup
    private float cellWidth;
    private float cellHeight;
    private Cell[,] flickCellGrid;
    private RectTransform containerRect;

    // refs
    [SerializeField]
    private GameObject buttonPrefab;
    [HideInInspector]
    public Keyboard keyboard;

    // flick button input
    public FlickInputHandler inputHandler = new FlickInputHandler();

    private void Awake()
    {
        flickCellGrid = new Cell[rows, columns];
        containerRect = transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
    }

    private FlickButton.Config GetConfigForGridCell(int r, int c)
    {
        // disabled button
        if (r == 0 && c == 2) 
        {
            var config = defaultButtonConfig.Clone();
            config.disabled = true;
            return config;
        }
        else
        {
            return defaultButtonConfig;
        }
    }

    private void SetUpButton(FlickButton button, int row, int col)
    {
        // per button setup 
        Action<string, string, string, string, string> setupButton =
        (string c, string u, string d, string l, string r) =>
        {
            button.charSetup = new FlickButton.CharSetup() 
                { centerChar = c, upChar = u, downChar = d, leftChar = l, rightChar = r };
            button.config = GetConfigForGridCell(row, col);
            button.parentFlickLayout = this;
            button.name = "button r" + row + "c" + col;
            button.Init();
        };

        switch (type)
        {
            case InputType.KeyHiragana:
                if (row == 0 && col == 0) { setupButton("小", "゛", "゜", "　", "　" ); }
                if (row == 0 && col == 1) { setupButton("わ", "ん", "　", "を", "ー" ); }
                if (row == 0 && col == 2) { setupButton("　", "　", "　", "　", "　" ); } 
                if (row == 1 && col == 0) { setupButton("ま", "む", "も", "み", "め" ); }
                if (row == 1 && col == 1) { setupButton("や", "ゆ", "よ", "　", "　" ); }
                if (row == 1 && col == 2) { setupButton("ら", "る", "ろ", "り", "れ" ); }
                if (row == 2 && col == 0) { setupButton("た", "つ", "と", "ち", "て" ); }
                if (row == 2 && col == 1) { setupButton("な", "ぬ", "の", "に", "ね" ); }
                if (row == 2 && col == 2) { setupButton("は", "ふ", "ほ", "ひ", "へ" ); }
                if (row == 3 && col == 0) { setupButton("あ", "う", "お", "い", "え" ); }
                if (row == 3 && col == 1) { setupButton("か", "く", "こ", "き", "け" ); }
                if (row == 3 && col == 2) { setupButton("さ", "す", "そ", "し", "せ" ); }
                break;
            case InputType.KeyKatakana:
                if (row == 0 && col == 0) { setupButton("小", "゛", "゜", "　", "　" );}
                if (row == 0 && col == 1) { setupButton("ワ", "ン", "　", "ヲ", "ー" );}
                if (row == 0 && col == 2) { setupButton("　", "　", "　", "　", "　" );}
                if (row == 1 && col == 0) { setupButton("マ", "ム", "モ", "ミ", "メ" );}
                if (row == 1 && col == 1) { setupButton("ヤ", "ユ", "ヨ", "　", "　" );}
                if (row == 1 && col == 2) { setupButton("ラ", "ル", "ロ", "リ", "レ" );}
                if (row == 2 && col == 0) { setupButton("タ", "ツ", "ト", "チ", "テ" );}
                if (row == 2 && col == 1) { setupButton("ナ", "ヌ", "ノ", "ニ", "ネ" );}
                if (row == 2 && col == 2) { setupButton("ハ", "フ", "ホ", "ヒ", "へ" );}
                if (row == 3 && col == 0) { setupButton("ア", "ウ", "オ", "イ", "エ" );}
                if (row == 3 && col == 1) { setupButton("カ", "ク", "コ", "キ", "ケ" );}
                if (row == 3 && col == 2) { setupButton("サ", "ス", "ソ", "シ", "セ" );}
                break;
            case InputType.KeyRomaji:
                // romaji is only a visualisation of on prompts and the keyboard display
            default:
                break;
        }
        if (overrideFontSize)
        {
            button.fontSize = fontSize;
        }
    }

    void Update()
    {
        if (inputHandler.keyboard == null) inputHandler.keyboard = keyboard;
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
                // make button
                GameObject go = Instantiate(buttonPrefab, transform);
                Cell cell = new Cell()
                {
                    button = go.GetComponent<FlickButton>(),
                    tranform = go.GetComponent<RectTransform>()
                };
                // setting the button up
                SetUpButton(cell.button, r, c);
                // get ref matrix for updating the transform of buttons
                SetGridPosForCell(cell.tranform, containerRect, r, c);
                flickCellGrid[r, c] = cell;
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
                SetGridPosForCell(flickCellGrid[r, c].tranform, containerRect, r, c);
                SetGridCellSizeForCell(flickCellGrid[r, c].tranform, cellHeight, cellWidth);
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