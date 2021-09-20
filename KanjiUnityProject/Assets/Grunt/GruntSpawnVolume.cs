using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = UnityEngine.Random;

[RequireComponent(typeof(BoxCollider))]
public class GruntSpawnVolume : MonoBehaviour
{
    [Serializable]
    public class GruntSpawnConfig
    {
        // spawn timing
        public float spawnPeriod = 4.3f;

        public float spawnPeriodInc = 0.03f;

        // speed
        public float gruntSpeed = 2;

        public float gruntSpeedInc = 0.1f;
        public float gruntSpeedRng = 0.5f;
        public int noOfgruntsToDestroy = 10;
    }

    public GruntSpawnConfig config = new GruntSpawnConfig();
    private float timeSinceLastSpawn = 0;
    private BoxCollider boxCollider;
    private bool canGenerate = true;
    private List<Grunt> grunts = new List<Grunt>();

    // unity references
    public Grunt gruntPrefab;

    // kanji loading

    // Start is called before the first frame update
    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        timeSinceLastSpawn = config.spawnPeriod;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!canGenerate) return;

        if (timeSinceLastSpawn < config.spawnPeriod)
        {
            timeSinceLastSpawn += Time.deltaTime;
        }
        else
        {
            SpawnGrunt();
            timeSinceLastSpawn = 0;
        }
    }

    private void SpawnGrunt()
    {
        var bounds = boxCollider.bounds;
        Vector3 spawnLoc = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        Grunt grunt = Instantiate(
            gruntPrefab,
            spawnLoc,
            new Quaternion()).GetComponent<Grunt>();

        float speedMax = config.gruntSpeed + config.gruntSpeedRng;
        float speedMin = config.gruntSpeed - config.gruntSpeedRng;
        grunt.speed = Random.Range(speedMin, speedMax);
        grunt.speed = Mathf.Clamp(grunt.speed, 0, speedMax);
        grunts.Add(grunt);
    }
}