using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KanjiLib.Prompts;

public class HitTargetSpawner : MonoBehaviour
{

    public class HitGroup
    {
        public BeatManager.Beat groupBeat;
        public HitTarget question = null;
        public List<HitTarget> answers = new List<HitTarget>();
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


    private Dictionary<PromptChar, GameObject> promptToGameObjectMap = new Dictionary<PromptChar, GameObject>();


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
                groupBeat = beatManager.GetNextBeatTimeStamp(2, BeatManager.Beat.BeatType.Beat, refBeat)
            };
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
        PromptChar questionChar = GameManager.Instance.KanjiDatabase.GetRandomPromptChar();
        group.question = SpawnOne(
            GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds),
            group.groupBeat,
            questionChar,
            HitTarget.Type.Question,
            group);
    }

    private void SpawnAnwsers(HitTarget questionTarget)
    {
        PromptChar questionChar = questionTarget.prompt;
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
            PromptChar p;
            if(i == correctAnswer)
            {
               p = Utils.Clone<PromptChar>(questionChar);
               p.displayType = PromptDisplayType.Romaji;
            }
            else
            {
                p = GameManager.Instance.KanjiDatabase.GetRandomPromptChar(questionChar);
                p.displayType = PromptDisplayType.Romaji;
            }
            group.answers.Add(SpawnOne(position, ansBeat, p, HitTarget.Type.Answer, group));
        }
    
    }

    private HitTarget SpawnOne(
        Vector3 position, 
        BeatManager.Beat beat, 
        PromptChar PromptChar, 
        HitTarget.Type type, 
        HitGroup group) 
    {
        HitTarget ht = Instantiate(
            hitTargetPrefab,
            position,
            Quaternion.identity,
            transform).GetComponent<HitTarget>();
        ht.Init(type, PromptChar, group, beat);
        return ht;
    }

    private void UpdateGroups(HitGroup group) 
    {
        groups.Remove(group);
    }

}
