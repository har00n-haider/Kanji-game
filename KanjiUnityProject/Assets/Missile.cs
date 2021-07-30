using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Missile : MonoBehaviour, IKanjiHolder
{
    public float speed = 0.1f;
    public TextMeshPro textMeshPro;
    public AudioClip explosionSound;

    public ParticleSystem explosionPrefab;

    public KanjiData kanji { get ; set ; }
    public bool selected { get ; set ; }

    public System.Action onDestroy;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
        textMeshPro.transform.rotation = Quaternion.LookRotation(
            Camera.main.transform.forward,
            Camera.main.transform.up);
    }

    public void SetKanji(KanjiData kanji) 
    {
        this.kanji = kanji;
        textMeshPro.text = kanji.meanings[0];
    }

    private void OnTriggerEnter(Collider other)
    {
        Target target = other.gameObject.GetComponent<Target>();
        if (target != null) target.TakeDamage();
        Destroy();
    }

    public void Destroy()
    {
        if (IsDestroyed()) return;

        AudioSource.PlayClipAtPoint(explosionSound, gameObject.transform.position);
        ParticleSystem explosion = Instantiate(
            explosionPrefab,
            gameObject.transform.position,
            gameObject.transform.rotation);
        Destroy(gameObject);
        //TODO: figure out to properly play an animation once
        Destroy(explosion.gameObject, explosionPrefab.main.duration - 2.3f);
        onDestroy?.Invoke();
    }

    public bool IsDestroyed()
    {
        return this == null;
    }
}
    