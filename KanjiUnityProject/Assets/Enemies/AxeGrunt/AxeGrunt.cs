using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AxeGrunt : MonoBehaviour, IPromptHolderControllable
{
    // configuration
    public float speed = 0.1f;

    public bool canMove = true;
    public PromptConfiguration promptConfig;
    public float attackInterval = 3f;
    public float attackCounter;

    [SerializeField]
    [Range(10f, 80f)]
    private float axeAngle = 45f;

    [SerializeField]
    private Vector3 axeSpin = Vector3.zero;

    // state
    private int health;

    private bool canAttack = false;

    // refs
    private MainCharacter mainCharacter = null;

    private Effect deathEffect;
    private Effect attackEffect;

    [SerializeField]
    private GameObject axePrefab;

    private void Awake()
    {
        mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter").GetComponent<MainCharacter>();
        Effect[] effects = GetComponents<Effect>();
        foreach (Effect effect in effects)
        {
            if (effect.effectName == "death effect") deathEffect = effect;
            if (effect.effectName == "attack effect") attackEffect = effect;
        }
        // allows grunt to immedeatley attack when in range
        attackCounter = attackInterval;
        canAttack = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (mainCharacter.health <= 0) Stop();
        if (canAttack) Attack();
    }

    private void Attack()
    {
        if (attackCounter < attackInterval)
        {
            attackCounter += Time.deltaTime;
        }
        else
        {
            // TODO choose something for this
            //attackEffect.StartEffect(mainCharacter.transform);
            ThrowAxe(mainCharacter.transform.position);
            attackCounter = 0;
        }
    }

    private void ThrowAxe(Vector3 point)
    {
        var velocity = PhysUtils.BallisticVelocity(transform.position, point, axeAngle);
        Axe axe = Instantiate(axePrefab, transform.position, Quaternion.identity).GetComponent<Axe>();
        axe.Init(mainCharacter);
        Rigidbody axeRb = axe.GetComponent<Rigidbody>();
        axeRb.transform.position = transform.position;
        axeRb.velocity = velocity;
        axeRb.angularVelocity = axeSpin;
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

    public void SetHealth(int health)
    {
        this.health = health;
    }

    public void TakeDamage(int damage)
    {
        if (health > 0) health -= damage;
        if (health <= 0) Destroy();
    }

    public Transform getTransform => transform;

    public bool isDestroyed => this == null;

    public PromptConfiguration getPromptConfig => promptConfig;

    public System.Action onDestroy { get; set; }

    #endregion IPromptHolderControllable implementation
}