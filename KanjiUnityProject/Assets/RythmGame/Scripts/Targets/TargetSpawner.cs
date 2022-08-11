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
    private BeatManager beatManager { get { return GameManager.Instance.BeatManager; } }
    private GameSettings settings { get { return GameManager.Instance.Settings; } }

    private List<SpawnData> toBeSpawnedTargets = new();
    private List<ITarget> spawnedTargets = new();

    [SerializeField]
    private string beatmapPath;

    public void Init(BeatMapData beatMapData)
    {
        LoadTargetData(beatMapData);
    }

    void LoadTargetData(BeatMapData beatMapData)
    {
        toBeSpawnedTargets = beatMapData.beatTargetData;
    }

    void Update()
    {
        SpawnTarget();
        CheckMissedTargets();
    }

    private void SpawnTarget()
    {
        if (toBeSpawnedTargets.Count <= 0) return;
        var targetToSpawn = toBeSpawnedTargets.FirstOrDefault(s => s.spawned == false);
        if (targetToSpawn == null) return;

        if (beatManager.IsBeatWithinRange(targetToSpawn.GetFirstBeat(), settings.spawnerConfig.spawnToBeatTimeOffset))
        {
            ITarget spawnedTarget = null;
            switch (targetToSpawn.type)
            {
                case TargetType.Basic:
                    BasicTargetSpawnData bd = targetToSpawn as BasicTargetSpawnData;
                    BasicTarget b = Instantiate(
                        emptyTargetPrefab,
                        NormalisedToSpawnVolumePosition(bd.normalisedPosition),
                        Quaternion.identity,
                        transform).GetComponent<BasicTarget>();
                    b.Init(bd, settings.basicTargetConfig);
                    b.OnBeatResult = HandleOnBeatResult;
                    spawnedTarget = b;
                    break;
                case TargetType.Draw:
                    CharacterTargetSpawnData cd = targetToSpawn as CharacterTargetSpawnData;
                    CharacterTarget characterTarget = Instantiate(
                        characterTargetPrefab,
                        spawnVolume.transform.TransformPoint(spawnVolume.center) - (settings.characterConfig.CharacterSize / 2), 
                        Quaternion.identity);
                    characterTarget.Init(cd, settings.characterConfig);
                    targetToSpawn.spawned = true;
                    break;
                case TargetType.Reading:
                    ReadTargetSpawnData rd = targetToSpawn as ReadTargetSpawnData;
                    ReadTarget rt = Instantiate(
                        readTargetPrefab,
                        NormalisedToSpawnVolumePosition(rd.normalisedPosition),
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
        List<Beat> beats = new();
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
            beats.Add(beatManager.GetNextHalfBeat(startBeatOffset, startBeat));
            beats.Add(beatManager.GetNextHalfBeat(endBeatOffset, startBeat));
        }

        CharacterTargetSpawnData csd = new CharacterTargetSpawnData 
        {
           normalisedPosition = spawnVolume.transform.TransformPoint(spawnVolume.center) - (settings.characterConfig.CharacterSize / 2),
           beats = beats,
           character = character,
           difficulty = difficulty,
           id = id,
        };
        toBeSpawnedTargets.Add(csd);
        return beats.Last();
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
