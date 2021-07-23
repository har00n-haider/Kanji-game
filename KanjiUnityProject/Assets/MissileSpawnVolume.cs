using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MissileSpawnVolume : MonoBehaviour
{

    public float intervalSeconds = 3;
    public float[] missileSpeeds = new float[4];
    private int missileSpeedLevel = 0;
    public KanjiManager kanjiManager;
    public Target target;

    public Missile missilePrefab;

    private BoxCollider boxCollider;
    private float elapsedSeconds = 0;

    private bool canGenerate = true;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

        elapsedSeconds = intervalSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canGenerate) return;

        if (elapsedSeconds < intervalSeconds)
        {
            elapsedSeconds += Time.deltaTime;
        }
        else 
        {
            SpawnMissile();
            elapsedSeconds = 0;
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
        Debug.Log(string.Format("difficulty increased {0}", missileSpeedLevel));
        if(missileSpeedLevel < (missileSpeeds.Length - 2)) 
        {
            intervalSeconds -= 1f;
            missileSpeedLevel++;
            return false;
        }
        return true;
    }

    void SpawnMissile() 
    {
        if (target == null) return;

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

        missile.speed = Random.Range(
            missileSpeeds[missileSpeedLevel], 
            missileSpeeds[missileSpeedLevel + 1]);

    }


}
