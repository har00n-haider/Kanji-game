using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;

public class TargetSpawner : MonoBehaviour
{

    public class HitGroup
    {
        public bool kanaToRomaji = false;
        public BeatManager.Beat groupBeat;
        public TapTarget question = null;
        public List<TapTarget> answers = new List<TapTarget>();
    }

    [SerializeField]
    private int MaxNoOfGroups;
    [SerializeField]
    private BoxCollider spawnVolume;
    [SerializeField]
    private GameObject hitTargetPrefab;

    private double spawnToBeatTimeOffset;

    private List<HitGroup> groups = new List<HitGroup>();

    private BeatManager beatManager { get { return GameManager.Instance.GameAudio.BeatManager; } }


    private Dictionary<Character, GameObject> promptToGameObjectMap = new Dictionary<Character, GameObject>();

    private bool tapTargetQuestionToggle = false;

    // vectors for answers
    readonly static float distanceFromQuestion = 4.2f;
    Vector3 up      = new Vector3( 0,  1, 0)            * distanceFromQuestion;
    Vector3 left    = new Vector3(-1, -1, 0).normalized * distanceFromQuestion;
    Vector3 right   = new Vector3( 1, -1, 0).normalized * distanceFromQuestion;

    // Start is called before the first frame update
    void Awake()
    {
        AppEvents.OnSelected += SpawnAnwsers;
        AppEvents.OnGroupCleared += UpdateGroups;

    }

    private void OnDestroy()
    {
        AppEvents.OnSelected -= SpawnAnwsers;
        AppEvents.OnGroupCleared -= UpdateGroups;
    }


    private void Start()
    {
        spawnToBeatTimeOffset = beatManager.BeatPeriod * 1.5;
    }


    // Update is called once per frame
    void Update()
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
            HitGroup group = new HitGroup()
            {
                groupBeat = beatManager.GetNextBeatTimeStamp(2, BeatManager.Beat.BeatType.Beat, refBeat),
                kanaToRomaji = tapTargetQuestionToggle
            };
            tapTargetQuestionToggle = !tapTargetQuestionToggle;
            groups.Add(group);
        }
        
        // check if any of the groups can be spawned
        foreach(HitGroup g in groups)
        {
            bool withinSpawnRange = beatManager.TimeToBeat(g.groupBeat) < spawnToBeatTimeOffset;
            if (g.question == null && withinSpawnRange)
            {
                SpawnQuestion(g);
            }   
        }
   
    }

    private void SpawnQuestion(HitGroup group) 
    {
        // question
        Character questionChar = GameManager.Instance.Database.GetRandomCharacter();
        questionChar.DisplayType = group.kanaToRomaji? DisplayType.Hiragana : DisplayType.Romaji;
        group.question = SpawnOne(
            GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds),
            group.groupBeat,
            questionChar,
            TapTarget.Type.Question,
            group);
    }

    private void SpawnAnwsers(TapTarget questionTarget)
    {
        Character questionChar = questionTarget.prompt;
        HitGroup group = questionTarget.group;

        // answers
        var ansBeat = beatManager.GetNextBeatTimeStamp(1, BeatManager.Beat.BeatType.Beat, group.groupBeat);
        int correctAnswer = Random.Range(0, 3);
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
            group.answers.Add(SpawnOne(position, ansBeat, p, TapTarget.Type.Answer, group));
        }
    
    }

    private TapTarget SpawnOne(
        Vector3 position, 
        BeatManager.Beat beat, 
        Character Character, 
        TapTarget.Type type, 
        HitGroup group) 
    {
        TapTarget ht = Instantiate(
            hitTargetPrefab,
            position,
            Quaternion.identity,
            transform).GetComponent<TapTarget>();
        ht.Init(type, Character, group, beat);
        return ht;
    }

    private void UpdateGroups(HitGroup group) 
    {
        groups.Remove(group);
    }

}
