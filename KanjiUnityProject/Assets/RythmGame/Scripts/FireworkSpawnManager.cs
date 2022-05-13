using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FireworkSpawnManager : MonoBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private GameObject _fireworkPrefab;

    [SerializeField]
    private float _minimumHorizontalForce;

    [SerializeField]
    private float _maximumHorizontalForce;

    [SerializeField]
    private float _minimumVerticalForce;

    [SerializeField]
    private float _maximumVerticalForce;

    [SerializeField]
    private float _minimumXTorque;
    [SerializeField]
    private float _maximumXTorque;

    [SerializeField]
    private float _minimumYTorque;
    [SerializeField]
    private float _maximumYTorque;

    [SerializeField]
    private float _minimumZTorque;
    [SerializeField]
    private float _maximumZTorque;

    private int _countOfSpawnPoints;
    private float _timeAccumulator;
    private float spawnInterval;

    // Start is called before the first frame update
    void Start()
    {
        Assert.IsNotNull(_fireworkPrefab);

        _countOfSpawnPoints = spawnPoints.Length;
        // Debug.Log($"_countOfSpawnPoints = {_countOfSpawnPoints}");
    }


    // Update is called once per frame
    void Update()
    {
        UpdateSpawnTime();

        var gameAudio = GameObject.FindWithTag("GameAudio")?.GetComponent<GameAudio>();
        if ( gameAudio != null && !gameAudio.IsSongPlaying()) return;

        _timeAccumulator += Time.deltaTime;
        if (_timeAccumulator > spawnInterval)
        {
            _timeAccumulator = 0;


            var horizForce = Random.Range(_minimumHorizontalForce, _maximumHorizontalForce);
            horizForce *= PickRandomPositiveOrNegative();

            var verticalForce = Random.Range(_minimumVerticalForce, _maximumVerticalForce);

            var xTorque = Random.Range(_minimumXTorque, _maximumXTorque);
            xTorque *= PickRandomPositiveOrNegative();
            var yTorque = Random.Range(_minimumYTorque, _maximumYTorque);
            yTorque *= PickRandomPositiveOrNegative();
            var zTorque = Random.Range(_minimumZTorque, _maximumZTorque);
            zTorque *= PickRandomPositiveOrNegative();

            // Spins cube clockwise, around it's y-axis.
            //float torque = 35.0f;
            //rb.AddTorque(spawnedFirework.transform.up * torque);

            // Spins cube forwards, around the x-axis
            //float torque = 35.0f;
            //rb.AddTorque(spawnedFirework.transform.right * torque);
            int spawnPointIndex = PickRandomSpawnPoint();

            Transform spawnPos = spawnPoints[spawnPointIndex];

            Vector3 fireworkLaunchForce = new Vector3(horizForce, verticalForce, 0);

            // Instantiate at position (0, 0, 0) and zero rotation.
            GameObject spawnedFirework = Instantiate(_fireworkPrefab, spawnPos.position, Quaternion.LookRotation(Vector3.up));
            
            var rb = spawnedFirework.GetComponent<Rigidbody>();

            rb.AddForce(fireworkLaunchForce);
            // Spins cube anti-clockwise, around the z-axis
            rb.AddTorque(spawnedFirework.transform.right * xTorque);
            rb.AddTorque(spawnedFirework.transform.up * yTorque);
            rb.AddTorque(spawnedFirework.transform.forward * zTorque);

            AppEvents.OnSpawnFirework?.Invoke(spawnedFirework, fireworkLaunchForce);
        }
    }

    // LEVEL DESIGNER???
    void UpdateSpawnTime() 
    {
        float t = Time.realtimeSinceStartup ;

        if (t < 30) { spawnInterval = 3f; }                   // warm up
        else if (t > 30 && t < 60) { spawnInterval = 1.5f; } // 1st minute
        else if (t > 60 && t < 120) { spawnInterval = 1f; }  // 2nd minute
        else if (t > 120 && t < 240) { spawnInterval = 0.8f; } // 3rd minute
        else if (t > 240) { spawnInterval = 0.6f; } // last stretch
    }

    int PickRandomSpawnPoint()
    {
        int index = Random.Range(0, _countOfSpawnPoints);

        // Debug.Log($"index = {index}");
        return index;
    }

    private float PickRandomPositiveOrNegative()
    {
        float f = Random.Range(0, 1.0f);

        if (f > 0.5f)
        {
            return 1.0f;
        }
        return -1.0f;
    }
}
