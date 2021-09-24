using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    // refs
    public GameObject bulletPrefab = null;

    // config
    public float personalSpaceDist;

    public int health;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void FireBullet(IPromptHolderControllable target)
    {
        Bullet b = Instantiate(bulletPrefab, transform.position, transform.rotation).GetComponent<Bullet>();
        b.Init(target);
    }

    public void Destroy()
    {
        Debug.Log("Dead");
    }

    public void TakeDamage(int damage)
    {
        if (health > 0) health -= damage;
        if (health <= 0) Destroy();
    }
}