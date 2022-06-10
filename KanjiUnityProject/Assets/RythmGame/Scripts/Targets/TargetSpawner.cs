using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;
using System;
using System.Linq;

using Random = UnityEngine.Random;

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


public enum Difficulty
{
    Easy,
    Normal,
    Hard,
    Insane
}

// TODO: split broadly into to two activities:
// - generating and assigning beats to data objects that hold information required to spawn interactable targets (e.g. groups below)
// - instantiating the required targets when required
public class TargetSpawner : MonoBehaviour
{
   // =========================== Writing group =========================== 
    [Header("Character target")]
    public CharacterTarget characterTargetPrefab;
    private List<CharacterTargetSpawnData> characterTargetsData = new List<CharacterTargetSpawnData>();
    [SerializeField]
    private CharacterConfig characterConfig;

    // =========================== Empty targets=========================== 
    [Header("Basic target")]
    [SerializeField]
    private GameObject emptyTargetPrefab;
    private List<BasicTargetSpawnData> basicTargetsData = new List<BasicTargetSpawnData>();
    [SerializeField]
    private BasicTargetConfig basicTargetConfig;

    // =========================== Reading group =========================== 

    [Header("Reading group")]
    [SerializeField]
    private GameObject readTargetPrefab;
    [SerializeField]
    private ReadTargetConfig readTargetConfig;
    private List<ReadTargetSpawnData> readTargetsData = new List<ReadTargetSpawnData>();

    // =========================== Spawner settings =========================== 

    // Spawing variables
    [Header("Spawning variables")]
    [SerializeField]
    private BoxCollider spawnVolume;
    [SerializeField]
    private float spawnToBeatTimeOffset;
    private BeatManager beatManager { get { return GameManager.Instance.GameAudio.BeatManager; } }


    // Start is called before the first frame update
    void Awake()
    {
    }

    private void OnDestroy()
    {
    }

    private void Start()
    {
        GenerateTargetData();
    }


    void Update()
    {
        SpawnStrokeTarget();
        SpawnBasicTarget();
        SpawnReadTarget();
    }

    void GenerateTargetData()
    {

        // Reading targets
        for (int i = 0; i < 10; i++)
        {
            Beat refBeat = null;
            // try to get a reference beat from the last group
            if (readTargetsData.Count > 0)
            {
                refBeat = readTargetsData.Last().questionBeat;
            }
            // make a new group some distance from this one
            Beat qB = beatManager.GetNextHalfBeat(4, refBeat);
            Beat aB = beatManager.GetNextHalfBeat(2, qB);
            CreateReadTargetData(GameManager.Instance.Database.GetRandomCharacter(), 
                qB,
                aB,
                Random.Range(0,2) == 1,
                GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds));
        }

        // empty targets
        for (int i = 0; i < 10; i++)
        {
            Beat refBeat = null;
            if (i == 0 && readTargetsData.Count > 0) refBeat = beatManager.GetNextHalfBeat(3, readTargetsData.Last().questionBeat);
            // try to get a reference beat from the last group
            else if (basicTargetsData.Count > 0)
            {
                refBeat = basicTargetsData.Last().beat;
            }
            CreateBasicTargetData(beatManager.GetNextHalfBeat(2, refBeat), 
                GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds));
        }

        // character targets
        for (int i = 0; i < 5; i++)
        {
            Beat refBeat = null;
            if (i == 0 && basicTargetsData.Count > 0) refBeat = beatManager.GetNextHalfBeat(3, basicTargetsData.Last().beat);
            // try to get a reference beat from the last group
            else if (characterTargetsData.Count > 0)
            {
                refBeat = characterTargetsData.Last().EndBeat;
            }

            if (i == 0) CreateDrawTargetData(beatManager.GetNextHalfBeat(4, refBeat), Difficulty.Easy);
            else if (i > 0 && i <= 3) CreateDrawTargetData(beatManager.GetNextHalfBeat(4, refBeat), Difficulty.Normal);
            else if (i > 3 && i <= 8) CreateDrawTargetData(beatManager.GetNextHalfBeat(4, refBeat), Difficulty.Hard);
            else if (i > 8) CreateDrawTargetData(beatManager.GetNextHalfBeat(4, refBeat), Difficulty.Insane);
        }
    }

    #region Draw targets

    private void CreateDrawTargetData(Beat startBeat, Difficulty difficulty)
    {
        Character character = characterConfig.overrideChar != ' ' ?
            GameManager.Instance.Database.GetCharacter(characterConfig.overrideChar) :
            GameManager.Instance.Database.GetRandomCharacter(null, CharacterType.hiragana);

        // generate the beats for the entire character
        List<Tuple<Beat, Beat>> beats = new();
        int beatIdx = -1;

        for (int i = 0; i < character.drawData.strokes.Count; i++)
        {
            int startBeatOffset = ++beatIdx;
            int endBeatOffset = startBeatOffset;

            // figure out how many beats you would need at the difficulty 
            float length = character.drawData.strokes[i].unscaledLength;
            float speed = 0.0f;
            switch (difficulty)
            {
                case Difficulty.Easy:
                    speed = characterConfig.speedEasy;
                    break;
                case Difficulty.Normal:
                    speed = characterConfig.speedNormal;
                    break;
                case Difficulty.Hard:
                    speed = characterConfig.speedHard;
                    break;
                case Difficulty.Insane:
                    speed = characterConfig.speedInsane;
                    break;
            }
            int beatsToComplete = (int) MathF.Ceiling((length / speed) / beatManager.HalfBeatPeriod);
            endBeatOffset += beatsToComplete;

            beatIdx = endBeatOffset;
            beats.Add(new Tuple<Beat, Beat>(
                beatManager.GetNextHalfBeat(startBeatOffset, startBeat),
                beatManager.GetNextHalfBeat(endBeatOffset, startBeat)
            ));                            
        }

        CharacterTargetSpawnData csd = new();
        csd.position = spawnVolume.transform.TransformPoint(spawnVolume.center) - (characterConfig.CharacterSize / 2);
        csd.beats = beats;
        csd.character = character;
        csd.difficulty = difficulty;
        csd.spawned = false;
        characterTargetsData.Add(csd);
    }

    private void SpawnStrokeTarget()
    {
        if (characterTargetsData.Count <= 0) return;
        
        var csd = characterTargetsData.First();
        if (beatManager.IsBeatWithinRange(csd.StartBeat, spawnToBeatTimeOffset))
        {
            CharacterTarget characterTarget = Instantiate(characterTargetPrefab, csd.position, Quaternion.identity);
            characterTarget.Init(csd, characterConfig);
            characterTargetsData.Remove(csd);
        }
    }

    #endregion

    #region Empty targets

    private void CreateBasicTargetData(Beat beat, Vector3 position)
    {
        basicTargetsData.Add(new BasicTargetSpawnData()
        {
            beat = beat,
            position = position,
            spawned = false
        }); ;
    }

    private void SpawnBasicTarget()
    {
        // check if any of the groups can be spawned
        foreach(BasicTargetSpawnData bd in basicTargetsData)
        {
            if (beatManager.IsBeatWithinRange(bd.beat, spawnToBeatTimeOffset) && !bd.spawned)
            {
                BasicTarget b = Instantiate(
                    emptyTargetPrefab,
                    bd.position,
                    Quaternion.identity,
                    transform).GetComponent<BasicTarget>();
                b.Init(bd.beat, basicTargetConfig);
                bd.spawned = true;
            }   
        }
    }


    #endregion

    #region Reading group
    
    private void CreateReadTargetData(Character questionChar, Beat questionBeat, Beat answerBeat, bool kanaToRomaji, Vector3 position)
    {
        ReadTargetSpawnData readTargetData = new();
        readTargetData.questionBeat = questionBeat;
        readTargetData.answerBeat = answerBeat;
        readTargetData.questionChar = questionChar;
        readTargetData.answers = new List<Character>();
        readTargetData.spawned = false;
        readTargetData.position = position;
        readTargetData.kanaToRomaji = kanaToRomaji;
        int correctAnswer = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            Character p;
            if (i == correctAnswer)
            {
               p = Utils.Clone(questionChar);
            }
            else
            {
                p = GameManager.Instance.Database.GetRandomCharacter(questionChar);
            }
            p.DisplayType = !readTargetData.kanaToRomaji? DisplayType.Hiragana : DisplayType.Romaji;
            readTargetData.answers.Add(p);
        }
        readTargetsData.Add(readTargetData);
    }

    private void SpawnReadTarget()
    {
        foreach (var rd in readTargetsData)
        {
            if (!rd.spawned && beatManager.IsBeatWithinRange(rd.questionBeat, spawnToBeatTimeOffset))
            {
                ReadTarget ht = Instantiate(
                    readTargetPrefab,
                    rd.position,
                    Quaternion.identity,
                    transform).GetComponent<ReadTarget>();
                ht.Init(rd, readTargetConfig);
                rd.spawned = true;
            }   
        }
    
    }

    #endregion 

}
