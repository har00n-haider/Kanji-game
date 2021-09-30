using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public abstract class EnemyBase : MonoBehaviour, IPromptHolderControllable
{
    private new Collider collider;

    public abstract Transform getTransform { get; }
    public abstract bool isDestroyed { get; }
    public abstract PromptConfiguration getPromptConfig { get; }
    public abstract Action onDestroy { get; set; }

    public abstract void AddHealth(int health);
    public abstract void Destroy();
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
    public abstract void OnCurrentPromptSet(Prompt prompt);
    public abstract void TakeDamage(int damage);
}

