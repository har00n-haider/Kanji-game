using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;
using System;
using System.Linq;


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
    public float hangaboutTimeCharacter;
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
    public float targetScale;
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
    private List<BasitTargetData> basitTargetData = new List<BasitTargetData>();
    [SerializeField]
    private BasicTargetConfig basicTargetConfig;


    // =========================== Reading group =========================== 
    /// <summary>
    /// Question and answers group for simple MCQ style kana test for reading
    /// </summary>
    public class KanaReadingGroup
    {
        public bool kanaToRomaji = false; // test can be two way 
        public Beat groupBeat;
        public ReadTarget question = null;
        public List<ReadTarget> answers = new List<ReadTarget>();
    }
    [Header("Reading group")]
    [SerializeField]
    private int MaxNoOfGroups;
    [SerializeField]
    private GameObject kanaReadingTargetPrefab;
    private bool tapTargetQuestionToggle = false;
    // vectors for answers
    Vector3 up      = new Vector3( 0,  1, 0)            * distanceFromQuestion;
    Vector3 left    = new Vector3(-1, -1, 0).normalized * distanceFromQuestion;
    Vector3 right   = new Vector3( 1, -1, 0).normalized * distanceFromQuestion;
    readonly static float distanceFromQuestion = 4.2f;
    private List<KanaReadingGroup> groups = new List<KanaReadingGroup>();


    // =========================== Spawner settings =========================== 

    // Spawing variables
    [Header("Spawning variables")]
    [SerializeField]
    private BoxCollider spawnVolume;
    private double spawnToBeatTimeOffset;
    private BeatManager beatManager { get { return GameManager.Instance.GameAudio.BeatManager; } }


    // Start is called before the first frame update
    void Awake()
    {
        AppEvents.OnSelected += SpawnAnwsers;
        AppEvents.OnGroupCleared += UpdateKanaReadingGroups;
    }

    private void OnDestroy()
    {
        AppEvents.OnSelected -= SpawnAnwsers;
        AppEvents.OnGroupCleared -= UpdateKanaReadingGroups;
    }

    private void Start()
    {
        spawnToBeatTimeOffset = beatManager.HalfBeatPeriod;
        GenerateTargetData();
    }


    void Update()
    {
        SpawnStrokeTarget();
        SpawnBasicTarget();
    }

    void GenerateTargetData()
    {
        // empty targets
        for (int i = 0; i < 10; i++)
        {
            Beat refBeat = null;
            // try to get a reference beat from the last group
            if (basitTargetData.Count > 0)
            {
                refBeat = basitTargetData[basitTargetData.Count - 1].beat;
            }
            CreateBasicTargetData(beatManager.GetNextHalfBeat(2, refBeat), GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds));
        }

        //// generate draw data
        //for (int i = 0; i < 13; i++)
        //{
        //    if (i == 0) CreateDrawTargetData(beatManager.GetNextHalfBeat(beatManager.NumHalfBeatsPerBar), Difficulty.Easy);
        //    else if (i > 0 && i <= 3) CreateDrawTargetData(beatManager.GetNextHalfBeat(4, characterTargetsData.Last().EndBeat), Difficulty.Normal);
        //    else if (i > 3 && i <= 8) CreateDrawTargetData(beatManager.GetNextHalfBeat(4, characterTargetsData.Last().EndBeat), Difficulty.Hard);
        //    else if (i > 8) CreateDrawTargetData(beatManager.GetNextHalfBeat(4, characterTargetsData.Last().EndBeat), Difficulty.Insane);
        //}
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
        characterTargetsData.Add(csd);
    }

    private void SpawnStrokeTarget()
    {
        if (characterTargetsData.Count <= 0) return;
        
        var csd = characterTargetsData.First();
        if (IsBeatWithinSpawnRange(csd.StartBeat))
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
        basitTargetData.Add(new BasitTargetData()
        {
            beat = beat,
            position = position
        });
    }

    private void SpawnBasicTarget()
    {
        // check if any of the groups can be spawned
        foreach(BasitTargetData et in basitTargetData)
        {
            bool withinSpawnRange = beatManager.TimeToBeat(et.beat) < spawnToBeatTimeOffset;
            if (withinSpawnRange && !et.spawned)
            {
                BasicTarget e = Instantiate(
                    emptyTargetPrefab,
                    et.position,
                    Quaternion.identity,
                    transform).GetComponent<BasicTarget>();
                e.Init(et.beat, basicTargetConfig);
                et.spawned = true;
            }   
        }
    }


    #endregion

    #region Reading group
    
    //TODO: switch to using events between the question and answer so that the question directly spawns answers when it 
    // is selected. 


    private void CreateReadingGroupns()
    {

        // fill up the groups with assigned beats 
        while (groups.Count < MaxNoOfGroups)
        {
            Beat refBeat = null;
            // try to get a reference beat from the last group
            if (groups.Count > 0)
            {
                refBeat = groups[groups.Count - 1].groupBeat;
            }
            // make a new group some distance from this one
            KanaReadingGroup group = new KanaReadingGroup()
            {
                groupBeat = beatManager.GetNextHalfBeat(2, refBeat),
                kanaToRomaji = tapTargetQuestionToggle
            };
            tapTargetQuestionToggle = !tapTargetQuestionToggle;
            groups.Add(group);
        }
        
        // check if any of the groups can be spawned
        foreach(KanaReadingGroup g in groups)
        {
            if (g.question == null && IsBeatWithinSpawnRange(g.groupBeat))
            {
                SpawnQuestion(g);
            }   
        }
    
    }

    // TODO: move this to the group
    private void SpawnQuestion(KanaReadingGroup group) 
    {
        // question
        Character questionChar = GameManager.Instance.Database.GetRandomCharacter();
        questionChar.DisplayType = group.kanaToRomaji? DisplayType.Hiragana : DisplayType.Romaji;
        group.question = SpawnOne(
            GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds),
            group.groupBeat,
            questionChar,
            ReadTarget.Type.Question,
            group);
    }

    private void SpawnAnwsers(ReadTarget questionTarget)
    {
        Character questionChar = questionTarget.prompt;
        KanaReadingGroup group = questionTarget.group;

        // answers
        var ansBeat = beatManager.GetNextHalfBeat(1, group.groupBeat);
        int correctAnswer = UnityEngine.Random.Range(0, 3);
        for (int i = 0; i < 3; i++)
        {
            Vector3 position = new Vector3();
            if (i == 0) position = group.question.transform.position + up;
            if (i == 1) position = group.question.transform.position + left;
            if (i == 2) position = group.question.transform.position + right;
            Character p;
            if (i == correctAnswer)
            {
               p = Utils.Clone(questionChar);
            }
            else
            {
                p = GameManager.Instance.Database.GetRandomCharacter(questionChar);
            }
            p.DisplayType = !group.kanaToRomaji? DisplayType.Hiragana : DisplayType.Romaji;
            group.answers.Add(SpawnOne(position, ansBeat, p, ReadTarget.Type.Answer, group));
        }
    
    }

    private ReadTarget SpawnOne(
        Vector3 position, 
        Beat beat, 
        Character Character, 
        ReadTarget.Type type, 
        KanaReadingGroup group) 
    {
        ReadTarget ht = Instantiate(
            kanaReadingTargetPrefab,
            position,
            Quaternion.identity,
            transform).GetComponent<ReadTarget>();
        ht.Init(type, Character, group, beat);
        return ht;
    }

    private void UpdateKanaReadingGroups(KanaReadingGroup group) 
    {
        groups.Remove(group);
    }

    #endregion 

    private bool IsBeatWithinSpawnRange(Beat beat)
    {
        return beatManager.TimeToBeat(beat) < spawnToBeatTimeOffset;
    }
}
