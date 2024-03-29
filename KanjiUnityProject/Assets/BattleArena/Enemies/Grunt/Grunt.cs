﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grunt : EnemyBase
{
    // configuration
    public float speed = 0.1f;

    public bool canMove = true;
    public float attackInterval = 2f;
    public float attackCounter;

    // state

    private bool canAttack = false;

    // refs
    private MainCharacter mainCharacter = null;

    private Effect deathEffect;
    private Effect attackEffect;

    public override void Awake()
    {
        base.Awake();
        mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter").GetComponent<MainCharacter>();
        Effect[] effects = GetComponents<Effect>();
        foreach (Effect effect in effects)
        {
            if (effect.effectName == "death effect") deathEffect = effect;
            if (effect.effectName == "attack effect") attackEffect = effect;
        }
        // allows grunt to immedeatley attack when in range
        attackCounter = attackInterval;
    }

    // Update is called once per frame
    private void Update()
    {
        if (mainCharacter.health <= 0) Stop();
        if (canMove) Move();
        if (canAttack) Attack();
    }

    private void Move()
    {
        gameObject.transform.LookAt(mainCharacter.transform);
        float distToPlayer = (mainCharacter.transform.position - transform.position).magnitude;
        if (distToPlayer > mainCharacter.personalSpaceDist)
        {
            canAttack = false;
            transform.position += gameObject.transform.forward * speed * Time.deltaTime;
        }
        else
        {
            canAttack = true;
        }
    }

    private void Attack()
    {
        if (attackCounter < attackInterval)
        {
            attackCounter += Time.deltaTime;
        }
        else
        {
            attackEffect.StartEffect(mainCharacter.transform);
            mainCharacter.TakeDamage(1);
            attackCounter = 0;
        }
    }

    private void Stop()
    {
        canAttack = false;
        canMove = false;
    }

    public override void Destroy()
    {
        if (this == null) return;
        deathEffect.StartEffect(transform);
        Destroy(gameObject);
        onDestroy?.Invoke();
    }

}