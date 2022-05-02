using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{

    [Serializable]
    public class SpawnerConfig
    {
        // spawn timing
        public float spawnPeriod = 3f;
        // speed
        public float gruntSpeed = 5;
        public float gruntSpeedRng = 3f;
        // enemy count
        public int noOfGrunts = 6;
        public int noOfAxeGrunts = 1;
    }

    // unity references
    private List<BoxCollider> boxColliders;
    public Grunt gruntPrefab;
    public AxeGrunt axeGruntPrefab;

    // state
    public SpawnerConfig config = new SpawnerConfig();
    private float timeSinceLastSpawn = 0;
    private List<IPromptHolderControllable> enemies = new List<IPromptHolderControllable>();
    private bool canSpawn = false;

    [HideInInspector]
    public int noOfGruntsSpawned;
    [HideInInspector]
    public int noOfAxeGruntsSpawned;

    // Start is called before the first frame update
    void Start()
    {
        boxColliders = new List<BoxCollider>(GetComponentsInChildren<BoxCollider>());
        // to start immediatley spawning
        timeSinceLastSpawn = config.spawnPeriod;
    }

    private void Update()
    {
        if (!canSpawn) return;
        if (timeSinceLastSpawn < config.spawnPeriod)
        {
            timeSinceLastSpawn += Time.deltaTime;
        }
        else
        {
            Spawn();
            timeSinceLastSpawn = 0;
        }
    }

    public void StartSpawning() 
    {
        ResetState();
        canSpawn = true;
    }

    public void ResetState()
    {
        noOfGruntsSpawned = 0;
        noOfAxeGruntsSpawned = 0;
        canSpawn = false;
        ClearEnemies();
    }

    private void ClearEnemies() 
    {
        foreach (var enemy in enemies)
        {
            enemy.Destroy();
        }
        enemies.Clear();
    }

    void OnEnemyDestroyed()
    {

    }

    void Spawn()
    {
        Vector3 spawnLoc = Vector3.zero;
        bool isOkToSpawn = false;
        while (!isOkToSpawn) 
        {
            // Choose a random collider
            var boxCollider = boxColliders.PickRandom();
            spawnLoc = new Vector3(
                Random.Range(boxCollider.bounds.min.x, boxCollider.bounds.max.x),
                Random.Range(boxCollider.bounds.min.y, boxCollider.bounds.max.y),
                Random.Range(boxCollider.bounds.min.z, boxCollider.bounds.max.z)
            );
            isOkToSpawn = true;
            foreach (IPromptHolderControllable enemy in enemies) 
            {
                if (enemy.getBounds().HasValue && enemy.getBounds().Value.Contains(spawnLoc)) 
                {
                    isOkToSpawn = false;
                    break;
                }
            }
        }

        // spawn the enemies
        if (config.noOfGrunts != noOfGruntsSpawned) SpawnGrunt(spawnLoc);
        if (config.noOfAxeGrunts != noOfAxeGruntsSpawned) SpawnAxeGrunt(spawnLoc);

    }

    void SpawnGrunt(Vector3 location) 
    {
        Grunt enemy = Instantiate(
            gruntPrefab,
            location,
            Quaternion.identity).GetComponent<Grunt>();
        // Randomly set their speed
        float speedMax = config.gruntSpeed + config.gruntSpeedRng;
        float speedMin = config.gruntSpeed - config.gruntSpeedRng;
        enemy.speed = Random.Range(speedMin, speedMax);
        enemy.speed = Mathf.Clamp(enemy.speed, 0, speedMax);
        enemies.Add(enemy);
        noOfGruntsSpawned++;
    }

    void SpawnAxeGrunt(Vector3 location)
    {
        AxeGrunt enemy = Instantiate(
            axeGruntPrefab,
            location,
            Quaternion.identity).GetComponent<AxeGrunt>();
        enemies.Add(enemy);
        noOfAxeGruntsSpawned++;
    }

}
