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
    public InputType type;
    private PromptChar curPromptChar = null;
    private char? lastChar = null;

    #region kana maps

    private static readonly Dictionary<char, char> katakanaToHiragana = new Dictionary<char, char>()
    {
        {'ア','あ'},
        {'イ','い'},
        {'ウ','う'},
        {'エ','え'},
        {'オ','お'},
        {'カ','か'},
        {'キ','き'},
        {'ク','く'},
        {'ケ','け'},
        {'コ','こ'},
        {'サ','さ'},
        {'シ','し'},
        {'ス','す'},
        {'セ','せ'},
        {'ソ','そ'},
        {'タ','た'},
        {'チ','ち'},
        {'ツ','つ'},
        {'テ','て'},
        {'ト','と'},
        {'ナ','な'},
        {'ニ','に'},
        {'ヌ','ぬ'},
        {'ネ','ね'},
        {'ノ','の'},
        {'ハ','は'},
        {'ヒ','ひ'},
        {'フ','ふ'},
        {'ヘ','へ'},
        {'ホ','ほ'},
        {'マ','ま'},
        {'ミ','み'},
        {'ム','む'},
        {'メ','め'},
        {'モ','も'},
        {'ヤ','や'},
        {'ユ','ゆ'},
        {'ヨ','よ'},
        {'ラ','ら'},
        {'リ','り'},
        {'ル','る'},
        {'レ','れ'},
        {'ロ','ろ'},
        {'ワ','わ'},
        {'ヰ','ゐ'},
        {'ヱ','ゑ'},
        {'ヲ','を'},
        {'ン','ん'},
        {'ガ','が'},
        {'ギ','ぎ'},
        {'グ','ぐ'},
        {'ゲ','げ'},
        {'ゴ','ご'},
        {'ザ','ざ'},
        {'ジ','じ'},
        {'ズ','ず'},
        {'ゼ','ぜ'},
        {'ゾ','ぞ'},
        {'ダ','だ'},
        {'ヂ','ぢ'},
        {'ヅ','づ'},
        {'デ','で'},
        {'ド','ど'},
        {'バ','ば'},
        {'ビ','び'},
        {'ブ','ぶ'},
        {'ベ','べ'},
        {'ボ','ぼ'},
        {'パ','ぱ'},
        {'ピ','ぴ'},
        {'プ','ぷ'},
        {'ペ','ぺ'},
        {'ポ','ぽ'},
        // yoon
        {'ャ','ゃ'},
        {'ュ','ゅ'},
        {'ョ','ょ'},
        // sokuon
        {'ッ','っ'},
        {'ー','ー'},
    };

    private static readonly Dictionary<char, char> hiraganaToKatana = new Dictionary<char, char>()
    {
        {'あ','ア'},
        {'い','イ'},
        {'う','ウ'},
        {'え','エ'},
        {'お','オ'},
        {'か','カ'},
        {'き','キ'},
        {'く','ク'},
        {'け','ケ'},
        {'こ','コ'},
        {'さ','サ'},
        {'し','シ'},
        {'す','ス'},
        {'せ','セ'},
        {'そ','ソ'},
        {'た','タ'},
        {'ち','チ'},
        {'つ','ツ'},
        {'て','テ'},
        {'と','ト'},
        {'な','ナ'},
        {'に','ニ'},
        {'ぬ','ヌ'},
        {'ね','ネ'},
        {'の','ノ'},
        {'は','ハ'},
        {'ひ','ヒ'},
        {'ふ','フ'},
        {'へ','ヘ'},
        {'ほ','ホ'},
        {'ま','マ'},
        {'み','ミ'},
        {'む','ム'},
        {'め','メ'},
        {'も','モ'},
        {'や','ヤ'},
        {'ゆ','ユ'},
        {'よ','ヨ'},
        {'ら','ラ'},
        {'り','リ'},
        {'る','ル'},
        {'れ','レ'},
        {'ろ','ロ'},
        {'わ','ワ'},
        {'ゐ','ヰ'},
        {'ゑ','ヱ'},
        {'を','ヲ'},
        {'ん','ン'},
        {'が','ガ'},
        {'ぎ','ギ'},
        {'ぐ','グ'},
        {'げ','ゲ'},
        {'ご','ゴ'},
        {'ざ','ザ'},
        {'じ','ジ'},
        {'ず','ズ'},
        {'ぜ','ゼ'},
        {'ぞ','ゾ'},
        {'だ','ダ'},
        {'ぢ','ヂ'},
        {'づ','ヅ'},
        {'で','デ'},
        {'ど','ド'},
        {'ば','バ'},
        {'び','ビ'},
        {'ぶ','ブ'},
        {'べ','ベ'},
        {'ぼ','ボ'},
        {'ぱ','パ'},
        {'ぴ','ピ'},
        {'ぷ','プ'},
        {'ぺ','ペ'},
        {'ぽ','ポ'},
        {'ゃ','ャ'},
        {'ゅ','ュ'},
        {'ょ','ョ'},
        {'っ','ッ'},
        {'ー','ー'},
    };
    
    #endregion

    public void SetPromptChar(PromptChar promptChar)
    {
        Reset();
        curPromptChar = promptChar;
    }

    private void Reset()
    {
        lastChar = null;
    }

    // to be called from the buttons
    public void UpdateCharacter(char newChar)
    {
        if (curPromptChar == null) return;

        if (IsModifier(newChar) && lastChar.HasValue)
        {
            HandleModifier(newChar);
        }
        else
        {
            lastChar = newChar;
        }

        //HACK: should use a delegate 
        keyboard.CharUpdated(lastChar.Value);
    }

    private void HandleModifier(char modChar)
    {
        // is there anything to modify?
        if (lastChar == null) return;
        // katakana conversion: convert to 
        char? charToMod = lastChar.Value;
        if (type == InputType.KeyKatakana) charToMod = katakanaToHiragana[charToMod.Value];
        switch (modChar)
        {
            case '小':
                charToMod = ApplySmall(charToMod.Value);
                break;
            case '゛':
                charToMod = ApplyHandakuten(charToMod.Value);
                break;
            case '゜':
                charToMod = ApplyDakuten(charToMod.Value);
                break;
            default:
                break;
        }
        if(charToMod != null) 
        {
            // katakana conversion: convert back
            if (type == InputType.KeyKatakana)
            {
                charToMod = hiraganaToKatana[charToMod.Value];
            }
            lastChar = charToMod;
        }
    }

    #region modifier helper methods

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

    private static char? ApplySmall(char inputChar)
    {
        switch (inputChar)
        {
            case 'や': return 'ゃ';
            case 'ゆ': return 'ゅ';
            case 'よ': return 'ょ';
            case 'つ': return 'っ';
            default:
                return null;
        }
    }

    private static char? ApplyHandakuten(char inputChar)
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
                return null;
        }
    }

    private static char? ApplyDakuten(char inputChar)
    {
        switch (inputChar)
        {
            case 'は': return 'ぱ';
            case 'ひ': return 'ぴ';
            case 'ふ': return 'ぷ';
            case 'へ': return 'ぺ';
            case 'ほ': return 'ぽ';
            default:
                return null;
        }
    }

    #endregion
}


/// <summary>
///  Hosts the flick buttons in responsive grid
/// </summary>
public class FlickLayout : MonoBehaviour
{
    private class Cell
    {
        public RectTransform transform;
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
    private FlickInputHandler inputHandler = new FlickInputHandler();

    private void Awake()
    {
        flickCellGrid = new Cell[rows, columns];
        containerRect = transform.parent.GetComponent<RectTransform>();
    }

    private void Start()
    {
    }

    private void Update()
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


    // change the keyboard to match required type
    public void SetType(InputType type) 
    {
        this.type = type; 
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                UpdateButtonChars(r, c);
            }
        }
        inputHandler.type = type;
    }

    public void SetPromptChar(PromptChar promptChar) 
    {
        inputHandler.SetPromptChar(promptChar);
    }
    
    // called by buttons
    public void UpdateFromInput(char newChar)
    {
        inputHandler.UpdateCharacter(newChar);
    }

    #region button setup

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

    private void UpdateButtonChars(int row, int col)
    {
        if (flickCellGrid.Length == 0) return;
        FlickButton button = flickCellGrid[row, col].button;
        // per button setup 
        Action<string, string, string, string, string> updateChars =
        (string c, string l, string u, string r, string d) =>
        {
            button.charSetup = new FlickButton.CharSetup()
            { centerChar = c, upChar = u, downChar = d, leftChar = l, rightChar = r };
            button.UpdateChars();
        };
        switch (type)
        {
            case InputType.KeyHiragana:

                if (row == 0 && col == 0) { updateChars("小", "　", "゛", "　", "゜"); }
                if (row == 0 && col == 1) { updateChars("わ", "を", "ん", "ー", "　"); }
                if (row == 0 && col == 2) { updateChars("　", "　", "　", "　", "　"); }
                if (row == 1 && col == 0) { updateChars("ま", "み", "む", "め", "も"); }
                if (row == 1 && col == 1) { updateChars("や", "　", "ゆ", "　", "よ"); }
                if (row == 1 && col == 2) { updateChars("ら", "り", "る", "れ", "ろ"); }
                if (row == 2 && col == 0) { updateChars("た", "ち", "つ", "て", "と"); }
                if (row == 2 && col == 1) { updateChars("な", "に", "ぬ", "ね", "の"); }
                if (row == 2 && col == 2) { updateChars("は", "ひ", "ふ", "へ", "ほ"); }
                if (row == 3 && col == 0) { updateChars("あ", "い", "う", "え", "お"); }
                if (row == 3 && col == 1) { updateChars("か", "き", "く", "け", "こ"); }
                if (row == 3 && col == 2) { updateChars("さ", "し", "す", "せ", "そ"); }
                break;
            case InputType.KeyKatakana:
                if (row == 0 && col == 0) { updateChars("小", "　", "゛", "　", "゜"); }
                if (row == 0 && col == 1) { updateChars("ワ", "ヲ", "ン", "ー", "　"); }
                if (row == 0 && col == 2) { updateChars("　", "　", "　", "　", "　"); }
                if (row == 1 && col == 0) { updateChars("マ", "ミ", "ム", "メ", "モ"); }
                if (row == 1 && col == 1) { updateChars("ヤ", "　", "ユ", "　", "ヨ"); }
                if (row == 1 && col == 2) { updateChars("ラ", "リ", "ル", "レ", "ロ"); }
                if (row == 2 && col == 0) { updateChars("タ", "チ", "ツ", "テ", "ト"); }
                if (row == 2 && col == 1) { updateChars("ナ", "ニ", "ヌ", "ネ", "ノ"); }
                if (row == 2 && col == 2) { updateChars("ハ", "ヒ", "フ", "へ", "ホ"); }
                if (row == 3 && col == 0) { updateChars("ア", "イ", "ウ", "エ", "オ"); }
                if (row == 3 && col == 1) { updateChars("カ", "キ", "ク", "ケ", "コ"); }
                if (row == 3 && col == 2) { updateChars("サ", "シ", "ス", "セ", "ソ"); }
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

    private void SetUpButton(int row, int col)
    {
        if (flickCellGrid.Length == 0) return;
        FlickButton button = flickCellGrid[row, col].button;
        button.config = GetConfigForGridCell(row, col);
        button.parentFlickLayout = this;
        button.name = "button r" + row + "c" + col;
        button.Init();
    }

    private void CreateFlickButtons()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                // make button
                GameObject go = Instantiate(buttonPrefab, transform);
                Cell cell = new Cell()
                {
                    button = go.GetComponent<FlickButton>(),
                    transform = go.GetComponent<RectTransform>()
                };
                // setting the button up
                flickCellGrid[r, c] = cell;
                SetUpButton(r, c);
                UpdateButtonChars(r, c);
                SetGridPosForCell(containerRect, r, c);
            }
        }
    }

    #endregion

    #region UI resizing 

    private void SetCellDimensions()
    {
        cellWidth = containerRect.rect.width / columns;
        cellHeight = containerRect.rect.height / rows;
    }

    private void UpdateGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                SetGridPosForCell(containerRect, r, c);
                SetGridCellSizeForCell(flickCellGrid[r, c].transform, cellHeight, cellWidth);
            }
        }
    }

    private void SetGridPosForCell(RectTransform parent, int r, int c)
    {
        RectTransform rect = flickCellGrid[r, c].transform;
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