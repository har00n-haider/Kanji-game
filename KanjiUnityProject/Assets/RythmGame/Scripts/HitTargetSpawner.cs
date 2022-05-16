using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KanjiLib.Prompts;

public class HitTargetSpawner : MonoBehaviour
{

    public class HitGroup
    {
        public HitTarget question;
        public List<HitTarget> answers = new List<HitTarget>();
    }

    [SerializeField]
    private BoxCollider spawnVolume;
    [SerializeField]
    private GameObject hitTargetPrefab;

    [SerializeField]
    private double spawnToBeatTimeOffset;

    private double nextBeatTimeStamp = -1;


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
    }

    private void OnDestroy()
    {
        AppEvents.OnSelected -= SpawnAnwsers;
    }

    // Update is called once per frame
    void Update()
    {
        if(nextBeatTimeStamp < 0 ) nextBeatTimeStamp = GameManager.Instance.GameAudio.BeatManager.GetNextBeatTimeStamp();

        bool validTime = nextBeatTimeStamp > 0;
        bool withinSpawnRange = nextBeatTimeStamp - AudioSettings.dspTime < spawnToBeatTimeOffset;
        if (withinSpawnRange && validTime) 
        {
            SpawnGroup();
            nextBeatTimeStamp = GameManager.Instance.GameAudio.BeatManager.GetNextBeatTimeStamp();
        }
    }

    private void SpawnGroup() 
    {
        HitGroup group = new HitGroup(); 

        // question
        PromptChar questionChar = GameManager.Instance.KanjiDatabase.GetRandomPromptChar();
        group.question = SpawnOne(
            GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds),
            nextBeatTimeStamp, 
            questionChar,
            HitTarget.Type.Question,
            group);

    }

    private void SpawnAnwsers(HitTarget questionTarget)
    {
        PromptChar questionChar = questionTarget.prompt;
        HitGroup group = questionTarget.group;

        // answers
        double ansBeatTimeStamp = GameManager.Instance.GameAudio.BeatManager.GetNextBeatTimeStamp();
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
            group.answers.Add(SpawnOne(position, ansBeatTimeStamp, p, HitTarget.Type.Answer, group));
        }
    
    }

    private HitTarget SpawnOne(Vector3 position, double timeStamp, PromptChar PromptChar, HitTarget.Type type, HitGroup group) 
    {
        HitTarget ht = Instantiate(
            hitTargetPrefab,
            position,
            Quaternion.identity,
            transform).GetComponent<HitTarget>();
        ht.Init(timeStamp, type, PromptChar, group);
        return ht;
    }

}
