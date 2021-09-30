using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grunt : EnemyBase
{
    // configuration
    public float speed = 0.1f;

    public bool canMove = true;
    public PromptConfiguration promptConfig;
    public float attackInterval = 2f;
    public float attackCounter;

    // state
    private int health;

    private bool canAttack = false;
    private bool promptSet = false;

    // refs
    private MainCharacter mainCharacter = null;

    private Effect deathEffect;
    private Effect attackEffect;
    private Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
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

    #region IPromptHolderControllable implementation

    public override void Destroy()
    {
        if (this == null) return;
        deathEffect.StartEffect(transform);
        Destroy(gameObject);
        onDestroy?.Invoke();
    }

    public override void AddHealth(int health)
    {
        this.health += health;
    }

    public override void TakeDamage(int damage)
    {
        if (health > 0) health -= damage;
        if (health <= 0) Destroy();
    }

    public override void OnCurrentPromptSet(Prompt prompt)
    {
        promptSet = true;
    }

    public override Transform getTransform => transform;

    public override bool isDestroyed => this == null;

    public override PromptConfiguration getPromptConfig => promptSet ? null : promptConfig;

    public override System.Action onDestroy { get; set; }


    #endregion IPromptHolderControllable implementation
}