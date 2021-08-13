using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class Kanji : MonoBehaviour
{

    public class StrokeResult
    {
        public bool pass = false;
        // same order and size as the refpoints
        public List<float?> refPointDistances = new List<float?>();
        public int tightPointIdx = -1;
    }

    public class StrokePair
    {
        public Stroke inpStroke = null;
        public Stroke refStroke = null;
        public StrokeResult strokeResult = null;
        public bool isValid { get { return inpStroke.isValid && refStroke.isValid; } }
    }

    // config
    public int noRefPointsInStroke { get; private set; } = 5;

}

