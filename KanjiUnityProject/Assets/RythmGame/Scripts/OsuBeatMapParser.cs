using Manabu.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BeatMapData
{
    public List<SpawnData> beatTargetData = new();
    /// <summary>
    /// without extension
    /// </summary>
    public string songName;
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

    public class HitObject
    {
        public Vector2 normalizedPosition;
        public float timeSeconds;
        public int timeMilliSeconds;
        public float? spinnerEndTime = null;
        public OsuBeatMapParser.LegacyHitObjectType type;
    }

    private static BeatMapData parsedData = new BeatMapData();
    private static Dictionary<int, char> millisecToBeatText = new Dictionary<int, char>();

    /// <summary>
    /// https://osu.ppy.sh/wiki/en/Client/File_formats/Osu_%28file_format%29#hit-objects
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static BeatMapData Parse(string path)
    {
        int lineCount = 0;
        bool StartParsingHitObjects = false;
        bool populateLastData = false;
        // Read the file and display it line by line.  
        foreach (string line in System.IO.File.ReadLines(path))
        {
            if (lineCount >= 100) break; // HACK: remove hard limit for testing

            if (line.Contains("AudioFilename"))
            {
                string name = line.Split("AudioFilename:")[1].Trim();
                parsedData.songName = Path.GetFileNameWithoutExtension(name);
                continue;
            }

            if (line.Contains("[HitObjects]"))
            {
                StartParsingHitObjects = true;
                continue;
            }

            // Using the tags section as a place to store the characters that will be used
            // in the character based beat targets
            // e.g. 14739|ざ,
            if (line.Contains("Tags:"))
            {
                var beatTextStrings = line.Split("Tags:")[1].Split(',');
                for (int i = 0; i < beatTextStrings.Length; i++)
                {
                    string[] textArray = beatTextStrings[i].Split("|");
                    millisecToBeatText[int.Parse(textArray[0])] = textArray[1][0];
                }
            }

            if (StartParsingHitObjects)
            {
                // parse all the details of this hit object
                var lineArray = line.Split(',');
                HitObject hitObject = new HitObject();
                hitObject.normalizedPosition =
                    new Vector2(float.Parse(lineArray[0]) / 512, (384 - float.Parse(lineArray[1])) / 384);
                hitObject.timeMilliSeconds = int.Parse(lineArray[2]);
                hitObject.timeSeconds = (float)hitObject.timeMilliSeconds / 1000;
                hitObject.type = (LegacyHitObjectType)int.Parse(lineArray[3]);

                // find out our beat target type using the hit sample data to represent the type
                TargetType type = TargetType.Basic;
                switch (lineArray[5])
                {
                    case "0:0:0:0:":
                        type = TargetType.Basic;
                        break;
                    case "0:1:0:0:":
                        type = TargetType.Reading;
                        break;
                    case "0:2:0:0:":
                        type = TargetType.Draw;
                        break;
                }

                // add to list out if finished populating last data
                if (!populateLastData)
                {
                    parsedData.beatTargetData.Add(CreateNewData(type, hitObject, ref populateLastData));
                }
                else
                {
                    SpawnData lastData = parsedData.beatTargetData[parsedData.beatTargetData.Count - 1];
                    // continue populating the old type
                    if (lastData.type == type)
                    {
                        switch (lastData.type)
                        {
                            case TargetType.Draw:
                                var writeTargetData = lastData as CharacterTargetSpawnData;
                                writeTargetData.beats.Add(new Beat(hitObject.timeSeconds));
                                break;
                            case TargetType.Reading:
                                var readTargetData = lastData as ReadTargetSpawnData;
                                readTargetData.answerBeat = new Beat(hitObject.timeSeconds);
                                populateLastData = false; // only ever add two beats to a reading input
                                break;
                        }
                    }
                    // last data is complete we need to create new data
                    else
                    {
                        parsedData.beatTargetData.Add(CreateNewData(type, hitObject, ref populateLastData));
                    }
                }


            }
            lineCount++;
        }

        foreach (SpawnData s in parsedData.beatTargetData) 
        {
            ValidateData(s);
        }

        Debug.Log($"Parsed {lineCount} lines, with {parsedData.beatTargetData.Count} beat objects");
        return parsedData;

    }

    private static SpawnData CreateNewData(TargetType type, HitObject firstHitObject, ref bool populatingLastData)
    {
        SpawnData data = null;
        // start populating with new data
        switch (type)
        {
            case TargetType.Basic:
                var basicTargetSpawnData = new BasicTargetSpawnData();
                basicTargetSpawnData.type = TargetType.Basic;
                basicTargetSpawnData.beat = new Beat(firstHitObject.timeSeconds);
                basicTargetSpawnData.normalisedPosition = firstHitObject.normalizedPosition;
                data = basicTargetSpawnData;
                populatingLastData = false; // will only ever have one hitobject worth of data
                break;
            case TargetType.Draw:
                // we don't currently manually position the character in the middle of the screen
                var writeTargetData = new CharacterTargetSpawnData();
                writeTargetData.type = TargetType.Draw;
                writeTargetData.beats.Add(new Beat(firstHitObject.timeSeconds));
                writeTargetData.character = GameManager.Instance.Database.GetCharacter(millisecToBeatText[firstHitObject.timeMilliSeconds]);
                data = writeTargetData;
                populatingLastData = true;
                break;
            case TargetType.Reading:
                var readTargetData = new ReadTargetSpawnData();
                readTargetData.type = TargetType.Reading;
                readTargetData.questionBeat = new Beat(firstHitObject.timeSeconds);
                readTargetData.normalisedPosition = firstHitObject.normalizedPosition;
                // TODO: need to pull this from a seperate list
                readTargetData.character = GameManager.Instance.Database.GetRandomCharacter();
                data = readTargetData;
                populatingLastData = true;
                break;
        }
        return data;
    }

    private static void ValidateData(SpawnData data)
    {
        // were finished populating the old type, so add it to the list
        // some validation to the completed type
        switch (data.type)
        {
            case TargetType.Basic:
                var basicTargetdata = data as BasicTargetSpawnData;
                bool beatCheck = basicTargetdata.beat != null;
                if (!beatCheck) throw new Exception("Beat not assigned to basic target");
                break;
            case TargetType.Draw:
                var writeTargetData = data as CharacterTargetSpawnData;
                bool beatToStrokeCheck =
                    writeTargetData.beats.Count == writeTargetData.character.drawData.strokes.Count * 2;
                if (!beatToStrokeCheck) throw new Exception("Incorrect number of beats for character in character target");
                break;
            case TargetType.Reading:
                var readTargetData = data as ReadTargetSpawnData;
                bool qtoABeatCheck =
                    readTargetData.answerBeat != null &&
                    readTargetData.questionBeat != null;
                if (!qtoABeatCheck) throw new Exception("Incorrect number of beats in read target");
                break;
        }
    }


}
