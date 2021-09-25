using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : MonoBehaviour, IPromptHolderControllable
{
    public PromptConfiguration promptConfig;

    private MainCharacter target;

    private Effect hitEffect;

    // Start is called before the first frame update
    private void Start()
    {
        hitEffect = GetComponent<Effect>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
    }

    public void Init(MainCharacter target)
    {
        this.target = target;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform == target.transform)
        {
            target.TakeDamage(1);
            hitEffect.StartEffect(target.transform);
            Destroy(gameObject);
            onDestroy?.Invoke();
        }
    }

    #region IPromptHolderControllable

    public void Destroy()
    {
        throw new System.NotImplementedException();
    }

    public void SetHealth(int health)
    {
    }

    public void TakeDamage(int damage)
    {
        throw new System.NotImplementedException();
    }

    public Transform getTransform => throw new System.NotImplementedException();

    public bool isDestroyed => throw new System.NotImplementedException();

    public PromptConfiguration getPromptConfig => promptConfig;

    public System.Action onDestroy { get; set; }

    #endregion IPromptHolderControllable
}