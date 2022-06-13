using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OsuBeatMapImporter
{
    [Flags]
    public enum LegacyHitObjectType
    {
        Circle = 1,                                     // 1
        Slider = 1 << 1,                                // 2
        NewCombo = 1 << 2,                              // 4
        Spinner = 1 << 3,                               // 8
        ComboOffset = (1 << 4) | (1 << 5) | (1 << 6),
        Hold = 1 << 7
    }

    public class HitObject
    {
        public Vector2 position;
        public float timeSeconds;
        public LegacyHitObjectType type;
    }


    public static List<HitObject> ParseBeatMap(string path)
    {
        List<HitObject> hitObjects = new List<HitObject>();
        int lineCount = 0;
        bool StartParsingHitObjects = false;
        // Read the file and display it line by line.  
        foreach (string line in System.IO.File.ReadLines(path))
        {
            if (line.Contains("[HitObjects]"))
            {
                StartParsingHitObjects = true;
                continue;
            }
            if (StartParsingHitObjects)
            {
                HitObject h = new();
                var x = line.Split(',');

                h.position = new Vector2(float.Parse(x[0]) / 512, (384 - float.Parse(x[1])) / 384); 
                h.timeSeconds = float.Parse(x[2]) / 1000;
                h.type = (LegacyHitObjectType)int.Parse(x[3]);

                hitObjects.Add(h);
            }
            lineCount++;
        }
        return hitObjects;
    }
}
