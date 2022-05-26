using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;
using System;


// TODO: split broadly into to two activities:
// - generating and assigning beats to data objects that hold information required to spawn interactable targets (e.g. groups below)
// - instantiating the required targets when required
public class TargetSpawner : MonoBehaviour
{
   // =========================== Writing group =========================== 
    [Header("Writing group")]
    public CharacterTarget characterTargetPrefab;
    private List<CharacterTarget> kanawritingGroups = new List<CharacterTarget>();
    public Vector3 CharacterSize;
    public DrawableStrokeConfig WritingConfig { get { return writingConfig; } }
    [SerializeField]
    private DrawableStrokeConfig writingConfig;

    // =========================== Reading group =========================== 
    /// <summary>
    /// Question and answers group for simple MCQ style kana test for reading
    /// </summary>
    public class KanaReadingGroup
    {
        public bool kanaToRomaji = false; // test can be two way 
        public BeatManager.Beat groupBeat;
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


    // =========================== Empty targets=========================== 
    public class EmptyTargetEntry
    {
        public BeatManager.Beat beat;
        public bool spawned = false;
    }
    [Header("Empty target group")]
    // Empty target variables
    [SerializeField]
    private int MaxNoOfEmptyTargets;
    [SerializeField]
    private GameObject emptyTargetPrefab;
    private List<EmptyTargetEntry> emptyTargetBeats = new List<EmptyTargetEntry>();

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
        spawnToBeatTimeOffset = beatManager.BeatPeriod * 1.5;
        CreateDrawTarget();
    }

    // Update is called once per frame
    void Update()
    {
        SpawnNextStrokeTarget();
    }



    #region Draw targets

    private void CreateDrawTarget()
    {
        Character character = GameManager.Instance.Database.GetRandomCharacter(null, CharacterType.hiragana);
        // generate the beats for the entire character
        List<Tuple<BeatManager.Beat, BeatManager.Beat>> beats = new();
        int beatIdx = -1;
        var startBeat = beatManager.GetNextBeatTimeStamp(2, BeatManager.Beat.BeatType.Beat);
        for (int i = 0; i < character.drawData.strokes.Count; i++)
        {
            beats.Add(new Tuple<BeatManager.Beat, BeatManager.Beat>(
                beatManager.GetNextBeatTimeStamp(++beatIdx, BeatManager.Beat.BeatType.Beat, startBeat),
                beatManager.GetNextBeatTimeStamp(++beatIdx, BeatManager.Beat.BeatType.Beat, startBeat)
            ));                            
        }
        Vector3 position = spawnVolume.transform.TransformPoint(spawnVolume.center) - (CharacterSize / 2);
        CharacterTarget characterTarget = Instantiate(characterTargetPrefab, position, Quaternion.identity);
        characterTarget.Init(character, CharacterSize, beats);
        kanawritingGroups.Add(characterTarget);
    }

    private void SpawnNextStrokeTarget()
    {
        // check if any of the groups can be spawned
        foreach(CharacterTarget ct in kanawritingGroups)
        {
            foreach(var beatPair in ct.Beats)
            {
                if (IsBeatWithinSpawnRange(beatPair.Item1))
                {
                    ct.CreateNextStroke();
                }

            }
        } 
    }

    #endregion

    #region Empty targets

    private void CreateEmptyTargets()
    {
        // fill up the groups with assigned beats 
        while (emptyTargetBeats.Count < MaxNoOfEmptyTargets)
        {
            BeatManager.Beat refBeat = null;
            // try to get a reference beat from the last group
            if (emptyTargetBeats.Count > 0)
            {
                refBeat = emptyTargetBeats[emptyTargetBeats.Count - 1].beat;
            }
            emptyTargetBeats.Add(new EmptyTargetEntry()
            {
                beat = beatManager.GetNextBeatTimeStamp(1, BeatManager.Beat.BeatType.Beat, refBeat),
            });
        }
        
        // check if any of the groups can be spawned
        foreach(EmptyTargetEntry et in emptyTargetBeats)
        {
            bool withinSpawnRange = beatManager.TimeToBeat(et.beat) < spawnToBeatTimeOffset;
            if (withinSpawnRange && !et.spawned)
            {
                EmptyTarget e = Instantiate(
                    emptyTargetPrefab,
                    GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds),
                    Quaternion.identity,
                    transform).GetComponent<EmptyTarget>();
                e.Init(et.beat, UpdateEmptyTargetList);
                et.spawned = true;
            }   
        }

        Debug.Log("no of beats: " + emptyTargetBeats.Count );
    
    }

    private void UpdateEmptyTargetList(BeatManager.Beat beat) 
    {
        Debug.Log("Calling " + "UpdateEmptyTargetList");
        emptyTargetBeats.Remove( emptyTargetBeats.Find(et => et.beat == beat));
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
            BeatManager.Beat refBeat = null;
            // try to get a reference beat from the last group
            if (groups.Count > 0)
            {
                refBeat = groups[groups.Count - 1].groupBeat;
            }
            // make a new group some distance from this one
            KanaReadingGroup group = new KanaReadingGroup()
            {
                groupBeat = beatManager.GetNextBeatTimeStamp(2, BeatManager.Beat.BeatType.Beat, refBeat),
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
        var ansBeat = beatManager.GetNextBeatTimeStamp(1, BeatManager.Beat.BeatType.Beat, group.groupBeat);
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
        BeatManager.Beat beat, 
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


    private bool IsBeatWithinSpawnRange(BeatManager.Beat beat)
    {
        return beatManager.TimeToBeat(beat) < spawnToBeatTimeOffset;
    }
}
