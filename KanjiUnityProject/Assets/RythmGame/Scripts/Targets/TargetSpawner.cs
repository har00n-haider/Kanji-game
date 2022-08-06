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
    // refs
    public CharacterTarget characterTargetPrefab;
    [SerializeField]
    private GameObject emptyTargetPrefab;
    [SerializeField]
    private GameObject readTargetPrefab;
    [SerializeField]
    private BoxCollider spawnVolume;
    private BeatManager beatManager { get { return GameManager.Instance.GameAudio.BeatManager; } }
    private GameSettings settings { get { return GameManager.Instance.Settings; } }

    private List<SpawnData> toBeSpawnedTargets = new();
    private List<ITarget> spawnedTargets = new();

    [SerializeField]
    private string beatmapPath;

    private void Start()
    {
        //GenerateTargetData();
        LoadTargetData();
    }

    void LoadTargetData()
    {
        var hitObjects = OsuBeatMapImporter.ParseBeatMap(beatmapPath);
        for (int i = 1; i < hitObjects.Count; i+=2)
        {
            if (i > 0 && (i + 1) < hitObjects.Count)
            {
                var hitObject1 = hitObjects[i];
                var hitObject2 = hitObjects[i + 1];
                Beat beat1 = new Beat((double)hitObject1.timeSeconds, 0);
                Beat beat2 = new Beat((double)hitObject2.timeSeconds, 0);
                Vector3 position = NormalisedToSpawnVolumePosition(hitObject1.position);
                toBeSpawnedTargets.Add(new ReadTargetSpawnData(
                    position,
                    beat1,
                    beat2,
                    GameManager.Instance.Database.GetRandomCharacter(null, CharacterType.kanji),
                    Random.Range(0, 2) == 1
                ));
            }
        }
    }

    void Update()
    {
        SpawnTarget();
        // CheckMissedTargets();
    }

    private void SpawnTarget()
    {
        if (toBeSpawnedTargets.Count <= 0) return;
        var targetToSpawn = toBeSpawnedTargets.FirstOrDefault(s => s.spawned == false);
        if (targetToSpawn == null) return;

        if (beatManager.IsBeatWithinRange(targetToSpawn.beat, settings.spawnerConfig.spawnToBeatTimeOffset))
        {
            Debug.Log(targetToSpawn.id + "spawned");

            ITarget spawnedTarget = null;
            switch (targetToSpawn.type)
            {
                case TargetType.Basic:
                    BasicTargetSpawnData bd = targetToSpawn as BasicTargetSpawnData;
                    BasicTarget b = Instantiate(
                        emptyTargetPrefab,
                        bd.position,
                        Quaternion.identity,
                        transform).GetComponent<BasicTarget>();
                    b.Init(bd, settings.basicTargetConfig);
                    b.OnBeatResult = HandleOnBeatResult;
                    spawnedTarget = b;
                    break;
                case TargetType.Draw:
                    CharacterTargetSpawnData cd = targetToSpawn as CharacterTargetSpawnData;
                    CharacterTarget characterTarget = Instantiate(characterTargetPrefab, cd.position, Quaternion.identity);
                    characterTarget.Init(cd, settings.characterConfig);
                    break;
                case TargetType.Reading:
                    ReadTargetSpawnData rd = targetToSpawn as ReadTargetSpawnData;
                    ReadTarget rt = Instantiate(
                        readTargetPrefab,
                        rd.position,
                        Quaternion.identity,
                        transform).GetComponent<ReadTarget>();
                    rt.Init(rd, settings.readTargetConfig);
                    spawnedTarget = rt;
                    break;
            }

            if(spawnedTarget != null)
            {
                spawnedTargets.Add(spawnedTarget);
                targetToSpawn.spawned = true;
            }
        }
    }

    public void CheckMissedTargets()
    {
        spawnedTargets.RemoveAll(target => {
            bool beatMissed = beatManager.IsBeatMissed(target.Beat);
            if(beatMissed) target.HandleBeatResult(BeatResult.Miss);
            return beatMissed;
        });
    }

    private void HandleOnBeatResult(ITarget target)
    {
        spawnedTargets.Remove(target);
    }

    private Beat CreateDrawTargetData(Beat startBeat, Difficulty difficulty, CharacterType type, int id)
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
            int beatsToComplete = (int)MathF.Ceiling((length / speed) / beatManager.HalfBeatPeriod);
            endBeatOffset += beatsToComplete;

            beatIdx = endBeatOffset;
            beats.Add(new Tuple<Beat, Beat>(
                beatManager.GetNextHalfBeat(startBeatOffset, startBeat),
                beatManager.GetNextHalfBeat(endBeatOffset, startBeat)
            ));
        }

        CharacterTargetSpawnData csd = new CharacterTargetSpawnData(
            spawnVolume.transform.TransformPoint(spawnVolume.center) - (settings.characterConfig.CharacterSize / 2),
            beats,
            character,
            difficulty,
            id
        );
        toBeSpawnedTargets.Add(csd);
        return beats.Last().Item2;
    }

    private Vector3 NormalisedToSpawnVolumePosition(Vector2 normalisedSpawnVolumePos)
    {
        Vector3 localPos = new Vector3(
            normalisedSpawnVolumePos.x * spawnVolume.size.x,
            normalisedSpawnVolumePos.y * spawnVolume.size.y,
            0
        );
        return spawnVolume.transform.TransformPoint(localPos - spawnVolume.size * 0.5f);
    }
}
