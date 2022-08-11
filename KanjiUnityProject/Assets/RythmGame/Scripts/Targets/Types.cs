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

public abstract class SpawnData
{
    public abstract Beat GetFirstBeat();
    public TargetType type = TargetType.Basic;
    public bool spawned = false;
    public int id = -1;
    /// <summary>
    /// Between 0 and 1
    /// </summary>
    public Vector3 normalisedPosition = Vector3.zero;
}

public class BasicTargetSpawnData : SpawnData
{
    public Beat beat;
    public override Beat GetFirstBeat()
    {
        return beat;
    }
}

/// <summary>
/// Question and answers group for simple MCQ style kana test for reading
/// </summary>
public class ReadTargetSpawnData : SpawnData
{
    public bool kanaToRomaji = true;
    public Beat questionBeat = null;
    public Beat answerBeat = null;
    public Character character;
    public List<Character> answers = new();
    public override Beat GetFirstBeat()
    {
        return questionBeat;
    }

}

public class CharacterTargetSpawnData : SpawnData
{
    public List<Beat> beats = new();
    public Character character = null;
    public Difficulty difficulty = Difficulty.Easy;
    public override Beat GetFirstBeat()
    {
        return beats.Count > 0 ? beats[0]: null;
    }
}


public interface ITarget
{
    //public int ID { get; }
    public void HandleBeatResult(BeatResult hitResult);
    public Action<ITarget> OnBeatResult { get; set; }
    public Beat Beat { get;}
}