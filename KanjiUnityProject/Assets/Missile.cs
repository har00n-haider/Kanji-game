using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Missile : MonoBehaviour, IKanjiHolder
{
    public float health = 1f;
    public float speed = 0.1f;
    public AudioClip explosionSound;
    public AudioClip ricochetSound;

    public ParticleSystem explosionPrefab;

    public KanjiData kanjiData { get ; set ; }
    public bool selected { get ; set ; }

    public System.Action onDestroy;

    public GameObject label;
    private RectTransform labelRect;
    private Text labelText;
    private Image labelImage;
    public Color labelColor;
    public Color textColor;

    public float labelOffsetYPercentage;


    public bool canMove = true;


    void Awake() 
    {
        labelRect = label.GetComponentInChildren<RectTransform>();
        labelText = label.GetComponentInChildren<Text>();
        labelImage = label.GetComponentInChildren<Image>();

        labelText.supportRichText = true;
        labelText.color = textColor;
        labelImage.color = labelColor;
        
    }


    // Start is called before the first frame update
    void Start()
    {
        UpdateLabel();
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove) gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
        UpdateLabel();
    }

    public void SetKanji(KanjiData kanji) 
    {
        this.kanjiData = kanji;
        labelText.text = kanji.meanings[0];
    }

    public void TakeDamage(float damage)
    {
        if (health < 0) return;

        health -= damage;
        if(health <= 0) 
        {
            Destroy();
        }
        else
        {
            AudioSource.PlayClipAtPoint(ricochetSound, gameObject.transform.position);
        }
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


    private void UpdateLabel() 
    {
        float labelRectW = labelRect.rect.width;
        float labelRectH = labelRect.rect.height;
        float sH = Camera.main.pixelHeight;
        float sW = Camera.main.pixelWidth;
        // update the location of the label on the screen
        var screenpoint = Camera.main.WorldToScreenPoint(transform.position);
        // smooth clamp rect to the screen dimensions
        float labelY = GeometryUtils.ClampLengthToRegion(screenpoint.y, labelRectH, sH);
        float labelX = GeometryUtils.ClampLengthToRegion(screenpoint.x, labelRectW, sW);
        // apply vertical offset to label
        float labelYOffset = labelOffsetYPercentage * sH;
        labelRect.position = new Vector2(labelX, labelY + labelYOffset);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (canMove) 
        {
            Target target = other.gameObject.GetComponent<Target>();
            if (target != null) target.TakeDamage();
            Destroy();
        }
    }

#if UNITY_EDITOR

    void OnDrawGizmos() 
    {
        float sphereRadius = 0.3f;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, sphereRadius);



    }


#endif

}
