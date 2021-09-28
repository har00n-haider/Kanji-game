﻿using System.Collections;
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

    public int health;
    public float healthBarOffsetScreenPercentage = -0.03f;

    // Start is called before the first frame update
    private void Start()
    {
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        healthBar = Instantiate(healthBarPrefab, mainCanvas.transform).GetComponent<HealthBar>();
        healthBarRect = healthBar.GetComponent<RectTransform>();

        healthBar.SetMaxHealth(health);
    }

    // Update is called once per frame
    private void Update()
    {
        Utils.UpdateLabelScreenPos(healthBarRect, healthBarOffsetScreenPercentage, transform.position);
    }

    public void FireBullet(IPromptHolderControllable target)
    {
        Bullet b = Instantiate(bulletPrefab, transform.position, transform.rotation).GetComponent<Bullet>();
        b.Init(target);
    }

    public void Destroy()
    {
        Debug.Log("Dead");
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
}