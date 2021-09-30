using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grunt : MonoBehaviour, IPromptHolderControllable
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

    public void Destroy()
    {
        if (this == null) return;
        deathEffect.StartEffect(transform);
        Destroy(gameObject);
        onDestroy?.Invoke();
    }

    public void AddHealth(int health)
    {
        this.health += health;
    }

    public void TakeDamage(int damage)
    {
        if (health > 0) health -= damage;
        if (health <= 0) Destroy();
    }

    public void OnCurrentPromptSet(Prompt prompt)
    {
        promptSet = true;
    }

    public Transform getTransform => transform;

    public bool isDestroyed => this == null;

    public PromptConfiguration getPromptConfig => promptSet ? null : promptConfig;

    public System.Action onDestroy { get; set; }

    public Bounds getBounds()
    {
        return collider.bounds;
    }

    #endregion IPromptHolderControllable implementation
}