using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class MissileSpawnVolume : MonoBehaviour
{

    public float intervalSeconds = 3;
    public float elapsedSeconds = 0;
    public KanjiManager kanjiManager;
    public Target target;

    public Missile missilePrefab;

    private BoxCollider boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

        elapsedSeconds = intervalSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        if (elapsedSeconds < intervalSeconds)
        {
            elapsedSeconds += Time.deltaTime;
        }
        else 
        {
            SpawnMissile();
            elapsedSeconds = 0;
        }


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

    }


}
