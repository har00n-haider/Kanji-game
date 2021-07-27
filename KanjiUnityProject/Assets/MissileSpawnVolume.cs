using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MissileSpawnVolume : MonoBehaviour
{

    // spawn timing
    public float spawnPeriod = 3;
    private float timeSinceLastSpawn = 0;
    private float spawnPeriodInterval = 0.1f;

    // unity references
    public Target target;
    public KanjiManager kanjiManager;
    public Missile missilePrefab;

    // speed
    public float missileSpeed { get { return missileSpeeds[mIdx]; } }
    public float[] missileSpeeds = new float[4];
    public float missileSpeedRng;
    private int mIdx = 0 ;

    private BoxCollider boxCollider;
    private bool canGenerate = true;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

        timeSinceLastSpawn = spawnPeriod;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canGenerate) return;
        if (target == null) return;

        if (timeSinceLastSpawn < spawnPeriod)
        {
            timeSinceLastSpawn += Time.deltaTime;
        }
        else 
        {
            SpawnMissile();
            timeSinceLastSpawn = 0;
        }

        if(kanjiManager.kanjiToBeCleared == 0) 
        {
            kanjiManager.IncrementClearRequirement();
            bool completed = IncreaseDifficulty();
            if (completed) 
            {
                canGenerate = false;
                Debug.Log("COMPLETED!!!");
            }
        }
    }
    
    // return: true == have hit the speed limit
    public bool IncreaseDifficulty() 
    {
        if(mIdx < (missileSpeeds.Length - 2)) 
        {
            float spawnPeriodNext = spawnPeriod - spawnPeriodInterval;
            spawnPeriod = spawnPeriodNext > 0 ? spawnPeriodNext : spawnPeriod;
            mIdx++;
            Debug.Log(string.Format("level  {0} - speed {1}",  mIdx, missileSpeed));
            return false;
        }
        return true;
    }

    void SpawnMissile() 
    {
        KanjiData kanji = kanjiManager.GetKanjiData();

        if (kanji == null) return;

        var bounds = boxCollider.bounds;
        Vector3 spawnLoc = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        Missile missile = Instantiate(
            missilePrefab, 
            spawnLoc, 
            new Quaternion()).GetComponent<Missile>();
        missile.gameObject.transform.LookAt(target.transform.position);
        missile.SetKanji(kanji);

        float speedMax = missileSpeed + missileSpeedRng;
        float speedMin = missileSpeed - missileSpeedRng;
        missile.speed = Random.Range(speedMin, speedMax);
        missile.speed = Mathf.Clamp(missile.speed, 0 , speedMax);

    }


}
