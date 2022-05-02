using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : EnemyBase
{
    // configuration
    public float rotationSpeedDegPerSecond;

    public float speed;

    // refs
    private MainCharacter target;

    private Effect hitEffect;
    private Effect deflectedEffect;
    private Transform axeMeshTransform;

    // Start is called before the first frame update
    private void Start()
    {
        base.Awake();
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

    // when the axe is deflected
    public override void Destroy()
    {
        if (this == null) return;
        deflectedEffect.StartEffect(transform);
        Destroy(gameObject);
        onDestroy?.Invoke();
    }


    public override void TakeDamage(int damage)
    {
        Destroy();
    }

}