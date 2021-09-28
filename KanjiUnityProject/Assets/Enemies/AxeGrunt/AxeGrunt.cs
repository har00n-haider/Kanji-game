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
    public PromptConfiguration promptConfigPhase1;
    public PromptConfiguration promptConfigPhase2;
    public bool phase2Configured = false; // required to use the same word as phase 1
    private Queue<PromptConfiguration> promptConfigurations = new Queue<PromptConfiguration>();
    public float attackInterval = 3f;

    [HideInInspector]
    public float attackCounter;

    public Color color;

    // state
    private int health;

    private bool canAttack = false;

    // refs
    private MainCharacter mainCharacter = null;

    private Effect deathEffect;

    [SerializeField]
    private GameObject axePrefab;

    private void Awake()
    {
        mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter").GetComponent<MainCharacter>();
        Effect[] effects = GetComponents<Effect>();
        foreach (Effect effect in effects)
        {
            if (effect.effectName == "death effect") deathEffect = effect;
        }
        // allows grunt to immedeatley attack when in range
        attackCounter = attackInterval;
        canAttack = true;

        // update color
        gameObject.GetComponentInChildren<Renderer>().material.color = color;

        // setup the first prompt
        promptConfigurations.Enqueue(promptConfigPhase1);
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
            // TODO: choose something for this
            //attackEffect.StartEffect(mainCharacter.transform);
            ThrowAxe(mainCharacter.transform.position);
            attackCounter = 0;
        }
    }

    private void ThrowAxe(Vector3 point)
    {
        Axe axe = Instantiate(axePrefab, transform.position, Quaternion.identity).GetComponent<Axe>();
        axe.Init(mainCharacter);
        axe.transform.LookAt(mainCharacter.transform);
    }

    private void Stop()
    {
        canAttack = false;
        canMove = false;
    }

    #region IPromptHolderControllable implementation

    public void OnCurrentPromptSet(Prompt prompt)
    {
        if (!phase2Configured)

        {
            promptConfigPhase2.useSpecificWord = true;
            promptConfigPhase2.word = prompt.words[0].hiragana;
            promptConfigurations.Enqueue(promptConfigPhase2);
            phase2Configured = true;
        }
    }

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

    public Transform getTransform => transform;

    public bool isDestroyed => this == null;

    public PromptConfiguration getPromptConfig
    {
        get
        {
            return promptConfigurations.Count > 0 ?
                promptConfigurations.Dequeue() : null;
        }
    }

    public System.Action onDestroy { get; set; }

    #endregion IPromptHolderControllable implementation
}