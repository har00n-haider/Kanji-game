using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MissileSpawnVolume : MonoBehaviour
{
    private class MissileSpawnConfig 
    {
        // spawn timing
        public float spawnPeriod = 3;
        public float spawnPeriodInc = 0.07f;
        // speed
        public float missileSpeed = 3;
        public float missileSpeedInc = 0.2f;
        public float missileSpeedRng = 1.5f;
        public int noOfMissilesToDestroy = 10;
    }

    private MissileSpawnConfig config = new MissileSpawnConfig();
    private float timeSinceLastSpawn = 0;
    private BoxCollider boxCollider;
    private bool canGenerate = true;
    private List<Missile> missiles = new List<Missile>();
    private int noOfMissileDestroyed;

    // unity references
    public Target target;
    public KanjiManager kanjiManager;
    public Missile missilePrefab;

    // kanji loading

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        timeSinceLastSpawn = config.spawnPeriod;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canGenerate) return;
        if (target == null) return;

        if (timeSinceLastSpawn < config.spawnPeriod)
        {
            timeSinceLastSpawn += Time.deltaTime;
        }
        else
        {
            SpawnMissile();
            timeSinceLastSpawn = 0;
        }
    }

    void OnMissileDestroyed() 
    {
        config.missileSpeed += config.missileSpeedInc;
        config.spawnPeriod -= config.spawnPeriodInc;
        noOfMissileDestroyed++;
        if (config.noOfMissilesToDestroy == noOfMissileDestroyed)
        {
            canGenerate = false;
            target.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

    void SpawnMissile()
    {
        KanjiData kanji = kanjiManager.database.GetRandomKanji();

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

        float speedMax = config.missileSpeed + config.missileSpeedRng;
        float speedMin = config.missileSpeed - config.missileSpeedRng;
        missile.speed = Random.Range(speedMin, speedMax);
        missile.speed = Mathf.Clamp(missile.speed, 0, speedMax);
        missile.onDestroy = OnMissileDestroyed;
        missiles.Add(missile);
    }


}
