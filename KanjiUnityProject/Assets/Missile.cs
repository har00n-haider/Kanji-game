using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Missile : MonoBehaviour, IKanjiHolder
{
    public float speed = 0.1f;
    public AudioClip explosionSound;

    public ParticleSystem explosionPrefab;

    public KanjiData kanji { get ; set ; }
    public bool selected { get ; set ; }

    public System.Action onDestroy;

    public GameObject label;
    private RectTransform labelRect;
    private Text labelText;
    private Image labelImage;
    public Color labelColor;

    public float labelOffsetY;

    void Awake() 
    {
        labelRect = label.GetComponentInChildren<RectTransform>();
        labelText = label.GetComponentInChildren<Text>();
        labelImage = label.GetComponentInChildren<Image>();

        labelText.supportRichText = true;
        labelImage.color = labelColor;
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;

        // update the location of the label on the screen
        var screenpoint = Camera.main.WorldToScreenPoint(transform.position);
        if (screenpoint.z >= 0)
        {
            labelRect.position = new Vector2(screenpoint.x, screenpoint.y + labelOffsetY);
        }
    }

    public void SetKanji(KanjiData kanji) 
    {
        this.kanji = kanji;
        labelText.text = kanji.meanings[0];
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
    