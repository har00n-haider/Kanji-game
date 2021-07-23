using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RawStroke 
{
    public int orderNo;
    public List<Vector2> keyPoints = new List<Vector2>();
    public List<Vector2> points = new List<Vector2>();
}

public class KanjiData 
{
    public enum CategoryType
    {
        kanjipower,
    }
    public string literal = string.Empty;
    public string code = string.Empty;
    public List<string> meanings = new List<string>();
    public List<string> readingsOn = new List<string>();
    public List<string> readingsKun = new List<string>();
    public string svgContent = string.Empty;
    public Tuple<CategoryType, string> category = null;
    public KanjiStats stats = new KanjiStats();
}

public class KanjiStats 
{
    public bool seen = false;
    public int timesCleared = 0;
}

public interface IKanjiHolder
{
    KanjiData kanji { get; set; }
    bool selected { get; set; }
    void Destroy();

    bool IsDestroyed();
}
