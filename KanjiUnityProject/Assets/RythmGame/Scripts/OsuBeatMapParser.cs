using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class BeatMapData
{
    public List<HitObject> hitObjects = new();
    /// <summary>
    /// without extension
    /// </summary>
    public string songName;
}



public class HitObject
{
    public Vector2 position;
    public float timeSeconds;
    public OsuBeatMapParser.LegacyHitObjectType type;
}

public class OsuBeatMapParser
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


    /// <summary>
    /// https://osu.ppy.sh/wiki/en/Client/File_formats/Osu_%28file_format%29#hit-objects
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static BeatMapData Parse(string path)
    {
        BeatMapData data = new BeatMapData();
        int lineCount = 0;
        bool StartParsingHitObjects = false;
        // Read the file and display it line by line.  
        foreach (string line in System.IO.File.ReadLines(path))
        {
            if (line.Contains("AudioFilename"))
            {
                string name = line.Split("AudioFilename:")[1].Trim();
                data.songName = Path.GetFileNameWithoutExtension(name);
                continue;
            }

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

                data.hitObjects.Add(h);
            }
            lineCount++;
        }
        return data;
    }
}
