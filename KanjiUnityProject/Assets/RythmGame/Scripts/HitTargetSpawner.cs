using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTargetSpawner : MonoBehaviour
{
    [SerializeField]
    private BoxCollider spawnVolume;
    [SerializeField]
    private GameObject hitTargetPrefab;

    [SerializeField]
    private double spawnToBeatTimeOffset;

    private double nextBeatTimeStamp = -1;

    [SerializeField]
    private int noOfTargetsToSpawn;
    private int noOfTargetsSpawned = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(nextBeatTimeStamp < 0 ) nextBeatTimeStamp = GameManager.Instance.GameAudio.BeatManager.GetNextBeatTimeStamp();

        bool validTime = nextBeatTimeStamp > 0;
        bool withinSpawnRange = nextBeatTimeStamp - AudioSettings.dspTime < spawnToBeatTimeOffset;
        bool canSpawn = noOfTargetsSpawned < noOfTargetsToSpawn;
        if (withinSpawnRange && canSpawn && validTime) 
        {
            Spawn();
            nextBeatTimeStamp = GameManager.Instance.GameAudio.BeatManager.GetNextBeatTimeStamp();
        }
    }

    private void Spawn() 
    {
        HitTarget ht = Instantiate(
            hitTargetPrefab,
            GeometryUtils.GetRandomPositionInBounds(spawnVolume.bounds),
            Quaternion.identity,
            transform).GetComponent<HitTarget>();
        ht.Init(nextBeatTimeStamp);
        noOfTargetsSpawned++;
    }
}
