using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Manabu.Core;
using System;
using System.Linq;

using Random = UnityEngine.Random;

// TODO: split broadly into to two activities:
// - generating and assigning beats to data objects that hold information required to spawn interactable targets (e.g. groups below)
// - instantiating the required targets when required
public class TargetSpawner : MonoBehaviour
{
   // =========================== Writing group =========================== 
    public CharacterTarget characterTargetPrefab;
    private List<CharacterTargetSpawnData> characterTargetsData = new List<CharacterTargetSpawnData>();
    // =========================== Empty targets=========================== 
    [SerializeField]
    private GameObject emptyTargetPrefab;
    private List<BasicTargetSpawnData> basicTargetsData = new List<BasicTargetSpawnData>();
    // =========================== Reading group =========================== 
    [SerializeField]
    private GameObject readTargetPrefab;
    private List<ReadTargetSpawnData> readTargetsData = new List<ReadTargetSpawnData>();
    // =========================== Spawner settings =========================== 
    // Spawing variables
    [SerializeField]
    private BoxCollider spawnVolume;

    // refs
    private BeatManager beatManager { get { return GameManager.Instance.GameAudio.BeatManager; } }
    private GameSettings settings { get { return GameManager.Instance.Settings; } }


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
        // starting beat
        Beat refBeat = beatManager.GetNextHalfBeat(4, null);
        for (int i = 0; i < 100; i++)
        {
            // Choose type
            //TargetType t = (TargetType) Random.Range(0, 3);
            TargetType t = (TargetType.Basic) ;
            // choose number
            int n = Random.Range(0, 5);
            for (int j = 0; j < n; j++)
            {
                refBeat = beatManager.GetNextHalfBeat(4, refBeat); // break between t all types
                switch (t)
                {
                    case TargetType.Reading:
                        // make a new group some distance from this one
                        Beat qB = refBeat;
                        Beat aB = beatManager.GetNextHalfBeat(4, qB);
                        CreateReadTargetData( 
                            qB,
                            aB,
                            Random.Range(0,2) == 1,
                            GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds),
                            CharacterType.katakana);
                        refBeat = aB;
                        break;
                    case TargetType.Basic:
                        CreateBasicTargetData(refBeat,GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds));
                        break;
                    case TargetType.Draw:
                        refBeat = CreateDrawTargetData(beatManager.GetNextHalfBeat(4, refBeat), Difficulty.Easy, CharacterType.katakana);
                        break;
                }
            }
        }
    }

    #region Draw targets

    private Beat CreateDrawTargetData(Beat startBeat, Difficulty difficulty, CharacterType type)
    {
        Character character = settings.characterConfig.overrideChar != ' ' ?
            GameManager.Instance.Database.GetCharacter(settings.characterConfig.overrideChar) :
            GameManager.Instance.Database.GetRandomCharacter(null, type);

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
                    speed = settings.characterConfig.speedEasy;
                    break;
                case Difficulty.Normal:
                    speed = settings.characterConfig.speedNormal;
                    break;
                case Difficulty.Hard:
                    speed = settings.characterConfig.speedHard;
                    break;
                case Difficulty.Insane:
                    speed = settings.characterConfig.speedInsane;
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
        csd.position = spawnVolume.transform.TransformPoint(spawnVolume.center) - (settings.characterConfig.CharacterSize / 2);
        csd.beats = beats;
        csd.character = character;
        csd.difficulty = difficulty;
        csd.spawned = false;
        characterTargetsData.Add(csd);

        return beats.Last().Item2;
    }

    private void SpawnStrokeTarget()
    {
        if (characterTargetsData.Count <= 0) return;
        
        var csd = characterTargetsData.First();
        if (beatManager.IsBeatWithinRange(csd.StartBeat, settings.spawnerConfig.spawnToBeatTimeOffset))
        {
            CharacterTarget characterTarget = Instantiate(characterTargetPrefab, csd.position, Quaternion.identity);
            characterTarget.Init(csd, settings.characterConfig);
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
            if (beatManager.IsBeatWithinRange(bd.beat, settings.spawnerConfig.spawnToBeatTimeOffset) && !bd.spawned)
            {
                BasicTarget b = Instantiate(
                    emptyTargetPrefab,
                    bd.position,
                    Quaternion.identity,
                    transform).GetComponent<BasicTarget>();
                b.Init(bd.beat, settings.basicTargetConfig);
                bd.spawned = true;
            }   
        }
    }


    #endregion

    #region Reading group
    
    private void CreateReadTargetData(Beat questionBeat, Beat answerBeat, bool kanaToRomaji, Vector3 position, CharacterType type)
    {
        ReadTargetSpawnData readTargetData = new();
        readTargetData.questionBeat = questionBeat;
        readTargetData.answerBeat = answerBeat;
        readTargetData.questionChar = GameManager.Instance.Database.GetRandomCharacter(null, type);
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
               p = Utils.Clone(readTargetData.questionChar);
            }
            else
            {
                p = GameManager.Instance.Database.GetRandomCharacter(readTargetData.questionChar, type);
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
            if (!rd.spawned && beatManager.IsBeatWithinRange(rd.questionBeat, settings.spawnerConfig.spawnToBeatTimeOffset))
            {
                ReadTarget ht = Instantiate(
                    readTargetPrefab,
                    rd.position,
                    Quaternion.identity,
                    transform).GetComponent<ReadTarget>();
                ht.Init(rd, settings.readTargetConfig);
                rd.spawned = true;
            }   
        }
    
    }

    #endregion 

}
