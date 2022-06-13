using Manabu.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BeatResult
{
    Miss = 0,
    Hit = 1
}

/// <summary>
/// Configuration for the how character targets behave and are visualized in game
/// </summary>
[Serializable]
public struct CharacterConfig
{
    [Header("Stroke target settings")]
    [Tooltip("Thickness of the stroke")]
    public float lineWidth;
    public float keyPointScale;
    public float followTargetScale;
    public float targetZOffset;
    [Tooltip("Distance between key points on a stroke. Determines the number of points in a stroke. Not scaled?")]
    public float keyPointDistance;
    [Tooltip("how long after the last stroke is completed, does the character stick around on in game")]
    public float hangaboutTime;
    [Tooltip("The scaling applied to the character stroke line points as the are by deafault between 0 - 1 (not the game object)")]
    public Vector3 CharacterSize;
    [Tooltip("Use this to override all generated characters to be this based on this")]
    public char overrideChar;

    [Header("Follow target - The thing you have to follow along the stroke")]
    public float followTargetBeatCircleRadiusBegin;
    public float followTargetBeatCircleLineWidth;
    public float followTargetRangeCircleLineWidth;
    public float followTargetColliderRadius;

    [Header("Difficulty - speed")]
    public float speedEasy;
    public float speedNormal;
    public float speedHard;
    public float speedInsane;

    [Header("Difficulty - visibility")]
    public float strokeVisibilityEasy;
    public float strokeVisibilityNormal;
    public float strokeVisibilityHard;
    public float strokeVisibilityInsane;
    public float strokeVisibilityFadeWidth;
}

/// <summary>
/// Configuration for the how basic targets behave and are visualized in game
/// </summary>
[Serializable]
public struct BasicTargetConfig
{
    public float lineWidth;
    public float targetScale;
    public float hangaboutTime;
    public float beatCircleRadiusBegin;
    public float beatMissedThreshold; // How long to stay alive if missed
}

/// <summary>
/// Configuration for the how read targets behave and are visualized in game
/// </summary>
[Serializable]
public struct ReadTargetConfig
{
    public float lineWidth;
    public float targetScale;
    public float hangaboutTime;
    public float beatCircleRadiusBegin;
    public float beatMissedThreshold; // How long to stay alive if missed
}


[Serializable]
public struct BeatManagerConfig
{
    public float beatHitAllowance;
}


[Serializable]
public struct SpawnerConfig
{
    [SerializeField]
    public float spawnToBeatTimeOffset;
}


[Serializable]
public struct GameSettings
{
    [SerializeField]
    public SpawnerConfig spawnerConfig;
    [SerializeField]
    public BeatManagerConfig beatManagerConfig;
    [SerializeField]
    public ReadTargetConfig readTargetConfig;
    [SerializeField]
    public CharacterConfig characterConfig;
    [SerializeField]
    public BasicTargetConfig basicTargetConfig;
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard,
    Insane
}

public enum TargetType
{
    Basic,
    Draw,
    Reading,
}

public class SpawnData
{
    public Beat beat;
    public TargetType type;
    public bool spawned = false;
    public int id = -1;
}

public class BasicTargetSpawnData : SpawnData
{
    public BasicTargetSpawnData(
        Beat beat,
        Vector3 position
    )
    {
        this.position = position;
        this.beat = beat;
        type = TargetType.Basic;
    }
    public Vector3 position;
}

/// <summary>
/// Question and answers group for simple MCQ style kana test for reading
/// </summary>
public class ReadTargetSpawnData : SpawnData
{
    public ReadTargetSpawnData(
        Vector3 position,
        Beat questionBeat,
        Beat answerBeat,
        Character character,
        bool kanaToRomaji
    )
    {
        this.position = position;
        this.questionBeat = beat = questionBeat;
        this.answerBeat =  answerBeat;
        this.character = character;
        this.kanaToRomaji = kanaToRomaji;
        type = TargetType.Reading;

        int correctAnswer = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            Character p;
            if (i == correctAnswer)
            {
                p = Utils.Clone(this.character);
            }
            else
            {
                p = GameManager.Instance.Database.GetRandomCharacter(this.character, character.type);
            }
            p.DisplayType = !kanaToRomaji ? DisplayType.Hiragana : DisplayType.Romaji;
            answers.Add(p);
        }
    }

    public bool kanaToRomaji;
    public Beat questionBeat;
    public Beat answerBeat;
    public Vector3 position;
    public Character character;
    public List<Character> answers = new();
}

public class CharacterTargetSpawnData : SpawnData
{

    public CharacterTargetSpawnData(
        Vector3 position,
        List<Tuple<Beat, Beat>> beats,
        Character character,
        Difficulty difficulty,
        int id
    )
    {
        this.position = position;
        this.beats = beats;
        this.character = character;
        this.difficulty = difficulty;
        beat = StartBeat;
        type = TargetType.Draw;
        this.id = id;
    }
    public Vector3 position;
    public List<Tuple<Beat, Beat>> beats;
    public Character character;
    public Difficulty difficulty;
    public Beat StartBeat { get { return beats.First().Item1; } }
    public Beat EndBeat { get { return beats.Last().Item2; } }
}


public interface ITarget
{
    //public int ID { get; }
    public void HandleBeatResult(BeatResult hitResult);
    public Action<ITarget> OnBeatResult { get; set; }
    public Beat Beat { get;}
}