using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    // refs
    public GameObject bulletPrefab;

    public GameObject healthBarPrefab;
    private HealthBar healthBar;
    private RectTransform healthBarRect;

    // config
    public float personalSpaceDist;

    public int health { get; private set; }

    public float healthBarOffsetScreenPercentage = -0.03f;

    // Start is called before the first frame update
    private void Start()
    {
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        healthBar = Instantiate(healthBarPrefab, mainCanvas.transform).GetComponent<HealthBar>();
        healthBarRect = healthBar.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    private void Update()
    {
        UIUtils.UpdateLabelScreenPos(healthBarRect, healthBarOffsetScreenPercentage, transform.position);
    }

    public void FireBullet(IPromptHolderControllable target)
    {
        Bullet b = Instantiate(bulletPrefab, transform.position, transform.rotation).GetComponent<Bullet>();
        b.Init(target);
    }

    public void Destroy()
    {

    }


    public void SetMaxHealth(int health)
    {
        healthBar.SetMaxHealth(health);
    }

    public void SetHealth(int health) 
    {
        this.health = health;
        healthBar.SetHealth(health);
    }

    public void TakeDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;
            healthBar.SetHealth(health);
        }

        if (health <= 0) Destroy();
    }

    public bool IsDead() 
    {
        return health <= 0 ? true : false;
    }
}