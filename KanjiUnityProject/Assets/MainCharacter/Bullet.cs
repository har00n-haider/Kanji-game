using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject target = null;
    public float speed = 0f;

    public void Init(GameObject target)
    {
        this.target = target;
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (target != null)
        {
            gameObject.transform.LookAt(target.transform);
            gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
        }
    }
}