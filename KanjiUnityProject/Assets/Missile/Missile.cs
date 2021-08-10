using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Missile : MonoBehaviour, IKanjiHolder
{
    // configuration
    public float maxHealth = 1f;
    public float curHealth;
    public float speed = 0.1f;

    // unity references
    public AudioClip explosionSound;
    public AudioClip ricochetSound;
    public ParticleSystem explosionPrefab;
    //public ParticleSystem ricochetPrefab;
    public GameObject label;
    public HealthBar healthBar;
    private RectTransform healthRect;
    public float healthOffsetYPercentage;

    // label stuff
    private RectTransform labelRect;
    private Text labelText;
    private Image labelImage;
    public Color labelColor;
    public Color textColor;
    public float labelOffsetYPercentage;

    public KanjiData kanjiData { get; set; }
    public bool selected { get; set; }
    public bool canMove = true;
    public System.Action onDestroy;

    void Awake() 
    {
        labelRect = label.GetComponentInChildren<RectTransform>();
        labelText = label.GetComponentInChildren<Text>();
        labelImage = label.GetComponentInChildren<Image>();

        labelText.supportRichText = true;
        labelText.color = textColor;
        labelImage.color = labelColor;

        healthRect = healthBar.GetComponent<RectTransform>();
    }


    // Start is called before the first frame update
    void Start()
    {
        curHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        UpdateLabel(healthRect, healthOffsetYPercentage);
        UpdateLabel(labelRect, labelOffsetYPercentage);
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove) gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
        UpdateLabel(healthRect, healthOffsetYPercentage);
        UpdateLabel(labelRect, labelOffsetYPercentage);
    }

    public void SetKanji(KanjiData kanji) 
    {
        this.kanjiData = kanji;
        labelText.text = kanji.meanings[0];
    }

    public void TakeDamage(float damage)
    {
        if (curHealth < 0) return;

        curHealth -= damage;
        healthBar.SetHealth(curHealth);
        if(curHealth <= 0) 
        {
            Destroy();
        }
        else
        {
            //ParticleSystem ricochet = Instantiate(
            //    ricochetPrefab,
            //    gameObject.transform.position,
            //    gameObject.transform.rotation);
            //Destroy(ricochet.gameObject, ricochetPrefab.main.duration);
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
        Destroy(explosion.gameObject, explosionPrefab.main.duration - 2.3f);
        onDestroy?.Invoke();
    }

    public bool IsDestroyed()
    {
        return this == null;
    }


    private void UpdateLabel(RectTransform lRect, float yOffsetPercentage) 
    {
        float labelRectW = lRect.rect.width;
        float labelRectH = lRect.rect.height;
        float sH = Camera.main.pixelHeight;
        float sW = Camera.main.pixelWidth;
        // update the location of the label on the screen
        var screenpoint = Camera.main.WorldToScreenPoint(transform.position);
        // smooth clamp rect to the screen dimensions
        float labelY = GeometryUtils.ClampLengthToRegion(screenpoint.y, labelRectH, sH);
        float labelX = GeometryUtils.ClampLengthToRegion(screenpoint.x, labelRectW, sW);
        // apply vertical offset to label
        float labelYOffset = yOffsetPercentage * sH;
        lRect.position = new Vector2(labelX, labelY + labelYOffset);
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
