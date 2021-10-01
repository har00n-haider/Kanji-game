using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO: check that this works
//[RequireComponent(typeof(Collider))]
public abstract class EnemyBase : MonoBehaviour, IPromptHolderControllable
{
    private new Collider collider;
    public PromptConfiguration promptConfig;
    private bool promptSet = false;
    private int health = 0;

    public virtual void Awake()
    {
        collider = GetComponent<Collider>();
    }

    #region IPromptHolderControllable implementation

    public Transform getTransform => transform;

    public bool isDestroyed => this == null;

    public virtual PromptConfiguration getPromptConfig => promptSet ? null : promptConfig;

    public Action onDestroy { get; set; }

    public void AddHealth(int health)
    {
        this.health += health;
    }

    public virtual void TakeDamage(int damage)
    {
        if (health > 0) health -= damage;
        if (health <= 0) Destroy();
    }

    public virtual Bounds? getBounds() 
    {
        if (isDestroyed) 
        {
            return null;
        }
        else
        {
            return collider.bounds;
        }
    }

    public virtual void OnCurrentPromptSet(Prompt prompt)
    {
        promptSet = true;
    }

    public abstract void Destroy();

    #endregion

}

