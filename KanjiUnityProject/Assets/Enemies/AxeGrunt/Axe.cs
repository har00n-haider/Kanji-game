using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour, IPromptHolderControllable
{
    // configuration
    public float rotationSpeedDegPerSecond;

    public float speed;

    public PromptConfiguration promptConfig;
    private bool promptSet = false;

    // refs
    private MainCharacter target;

    private Effect hitEffect;
    private Effect deflectedEffect;
    private Transform axeMeshTransform;
    private Collider collider;

    // Start is called before the first frame update
    private void Start()
    {
        collider = GetComponent<Collider>();
        Effect[] effects = GetComponents<Effect>();
        foreach (Effect effect in effects)
        {
            if (effect.effectName == "hit effect") hitEffect = effect;
            if (effect.effectName == "deflected effect") deflectedEffect = effect;
        }
        axeMeshTransform = transform.Find("AxeMeshes");
    }

    // Update is called once per frame
    private void Update()
    {
        axeMeshTransform.Rotate(Vector3.right, rotationSpeedDegPerSecond * Time.deltaTime);
        gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
    }

    public void Init(MainCharacter target)
    {
        this.target = target;
    }

    // when the axe hits the target
    private void OnTriggerEnter(Collider collider)
    {
        if (this == null) return;
        if (collider.transform == target.transform)
        {
            target.TakeDamage(1);
            hitEffect.StartEffect(target.transform);
            Destroy(gameObject);
            onDestroy?.Invoke();
        }
    }

    #region IPromptHolderControllable

    // when the axe is deflected
    public void Destroy()
    {
        if (this == null) return;
        deflectedEffect.StartEffect(transform);
        Destroy(gameObject);
        onDestroy?.Invoke();
    }

    public void AddHealth(int health)
    {
    }

    public void TakeDamage(int damage)
    {
        Destroy();
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

    #endregion IPromptHolderControllable
}