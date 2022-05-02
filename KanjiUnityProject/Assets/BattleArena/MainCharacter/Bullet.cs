using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public IPromptHolderControllable target = null;
    public float speed = 0f;

    private Effect hitEffect;

    public void Init(IPromptHolderControllable target)
    {
        this.target = target;
    }

    // Start is called before the first frame update
    private void Start()
    {
        hitEffect = GetComponent<Effect>();
    }

    // Update is called once per frame
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (!target.isDestroyed)
        {
            gameObject.transform.LookAt(target.getTransform);
            gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform == target.getTransform)
        {
            target.TakeDamage(1);
            hitEffect.StartEffect(collider.transform);
            Destroy(gameObject);
        }
    }
}