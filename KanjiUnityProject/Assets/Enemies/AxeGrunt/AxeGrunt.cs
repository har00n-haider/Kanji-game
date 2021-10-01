using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AxeGrunt : EnemyBase
{
    // configuration
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
    private bool canAttack = false;

    // refs
    private MainCharacter mainCharacter = null;

    private Effect deathEffect;

    [SerializeField]
    private GameObject axePrefab;

    private void Awake()
    {
        base.Awake();
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

    public override void OnCurrentPromptSet(Prompt prompt)
    {
        if (!phase2Configured)

        {
            promptConfigPhase2.useSpecificWord = true;
            promptConfigPhase2.word = prompt.words[0].hiragana;
            promptConfigurations.Enqueue(promptConfigPhase2);
            phase2Configured = true;
        }
    }

    public override void Destroy()
    {
        if (this == null) return;
        deathEffect.StartEffect(transform);
        Destroy(gameObject);
        onDestroy?.Invoke();
    }

    public override  PromptConfiguration getPromptConfig
    {
        get
        {
            return promptConfigurations.Count > 0 ?
                promptConfigurations.Dequeue() : null;
        }
    }

}