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
        bool requiresConversion = type == InputType.KeyKatakana || type == InputType.KeyKatakanaWithRomaji;
        if (requiresConversion) charToMod = katakanaToHiragana[charToMod.Value];
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
            if (requiresConversion)
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
        // helpers functions: per button setup 
        Action<string, string, string, string, string> setInputChars =
        (string c, string l, string u, string r, string d) =>
        {
            button.charSetup = new FlickButton.CharSetup()
            { 
                iCenterChar = c, 
                iUpChar = u, 
                iDownChar = d, 
                iLeftChar = l, 
                iRightChar = r,

                dCenterChar = c,
                dUpChar = u,
                dDownChar = d,
                dLeftChar = l,
                dRightChar = r
            };
            button.UpdateChars();
        };
        Action<string, string, string, string, string> addRomaji =
        (string c, string l, string u, string r, string d) =>
        {
            string romajiStr(string s) => "\n<size=50%>" + s.AddColor(Color.white);
            button.charSetup.dCenterChar = button.charSetup.iCenterChar + romajiStr(c);
            button.charSetup.dUpChar     = button.charSetup.iUpChar     + romajiStr(u);
            button.charSetup.dDownChar   = button.charSetup.iDownChar   + romajiStr(d);
            button.charSetup.dLeftChar   = button.charSetup.iLeftChar   + romajiStr(l);
            button.charSetup.dRightChar  = button.charSetup.iRightChar  + romajiStr(r);
            button.UpdateChars();
        };
        // helper functions: layout setup
        Action<int, int> setKatakanaLayout =
        (int r, int c) =>
        {
            if (r == 0 && c == 0) { setInputChars("小", "　", "゛", "　", "゜"); }
            if (r == 0 && c == 1) { setInputChars("ワ", "ヲ", "ン", "ー", "　"); }
            if (r == 0 && c == 2) { setInputChars("　", "　", "　", "　", "　"); }
            if (r == 1 && c == 0) { setInputChars("マ", "ミ", "ム", "メ", "モ"); }
            if (r == 1 && c == 1) { setInputChars("ヤ", "　", "ユ", "　", "ヨ"); }
            if (r == 1 && c == 2) { setInputChars("ラ", "リ", "ル", "レ", "ロ"); }
            if (r == 2 && c == 0) { setInputChars("タ", "チ", "ツ", "テ", "ト"); }
            if (r == 2 && c == 1) { setInputChars("ナ", "ニ", "ヌ", "ネ", "ノ"); }
            if (r == 2 && c == 2) { setInputChars("ハ", "ヒ", "フ", "へ", "ホ"); }
            if (r == 3 && c == 0) { setInputChars("ア", "イ", "ウ", "エ", "オ"); }
            if (r == 3 && c == 1) { setInputChars("カ", "キ", "ク", "ケ", "コ"); }
            if (r == 3 && c == 2) { setInputChars("サ", "シ", "ス", "セ", "ソ"); }
        };
        Action<int, int> setHiraganaLayout =
        (int r, int c) =>
        {
            if (r == 0 && c == 0) { setInputChars("小", "　", "゛", "　", "゜"); }
            if (r == 0 && c == 1) { setInputChars("わ", "を", "ん", "ー", "　"); }
            if (r == 0 && c == 2) { setInputChars("　", "　", "　", "　", "　"); }
            if (r == 1 && c == 0) { setInputChars("ま", "み", "む", "め", "も"); }
            if (r == 1 && c == 1) { setInputChars("や", "　", "ゆ", "　", "よ"); }
            if (r == 1 && c == 2) { setInputChars("ら", "り", "る", "れ", "ろ"); }
            if (r == 2 && c == 0) { setInputChars("た", "ち", "つ", "て", "と"); }
            if (r == 2 && c == 1) { setInputChars("な", "に", "ぬ", "ね", "の"); }
            if (r == 2 && c == 2) { setInputChars("は", "ひ", "ふ", "へ", "ほ"); }
            if (r == 3 && c == 0) { setInputChars("あ", "い", "う", "え", "お"); }
            if (r == 3 && c == 1) { setInputChars("か", "き", "く", "け", "こ"); }
            if (r == 3 && c == 2) { setInputChars("さ", "し", "す", "せ", "そ"); }

        };
        Action<int, int> addRomajiToLayout =
        (int r, int c) =>
        {
            // this are just hint texts added to the button
            if (row == 0 && col == 0) { addRomaji("", "", "", "", ""); }
            if (row == 0 && col == 1) { addRomaji("wa", "wo", "n", "", ""); }
            if (row == 0 && col == 2) { addRomaji("", "", "", "", ""); } 
            if (row == 1 && col == 0) { addRomaji("ma", "mi", "mu", "me", "mo"); }
            if (row == 1 && col == 1) { addRomaji("ya", "　", "yu", "　", "yo"); }
            if (row == 1 && col == 2) { addRomaji("ra", "ri", "ru", "re", "ro"); }
            if (row == 2 && col == 0) { addRomaji("ta", "chi", "tsu", "te", "to"); }
            if (row == 2 && col == 1) { addRomaji("na", "ni", "nu", "ne", "no"); }
            if (row == 2 && col == 2) { addRomaji("ha", "hi", "fu", "he", "ho"); }
            if (row == 3 && col == 0) { addRomaji("a", "i", "u", "e", "o"); }
            if (row == 3 && col == 1) { addRomaji("ka", "ki", "ku", "ke", "ko"); }
            if (row == 3 && col == 2) { addRomaji("sa", "shi", "su", "se", "so"); }
        };

        switch (type)
        {
            case InputType.KeyHiragana:
                setHiraganaLayout(row, col);
                break;
            case InputType.KeyKatakana:
                setKatakanaLayout(row, col);
                break;
            case InputType.KeyHiraganaWithRomaji:
                setHiraganaLayout(row, col);
                addRomajiToLayout(row, col);
                break;
            case InputType.KeyKatakanaWithRomaji:
                setKatakanaLayout(row, col);
                addRomajiToLayout(row, col);
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