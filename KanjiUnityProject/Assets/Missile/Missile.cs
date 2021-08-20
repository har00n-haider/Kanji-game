using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Missile : MonoBehaviour
{
    public float speed = 0.1f;

    // unity references
    public AudioClip explosionSound;
    public AudioClip ricochetSound;
    public ParticleSystem explosionPrefab;

    public bool canMove = true;
    public System.Action onDestroy;

    void Awake() 
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(canMove) gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
    }

    public void Destroy()
    {
        if (this == null) return;
        AudioSource.PlayClipAtPoint(explosionSound, gameObject.transform.position);
        ParticleSystem explosion = Instantiate(
            explosionPrefab,
            gameObject.transform.position,
            gameObject.transform.rotation);
        Destroy(gameObject);
        Destroy(explosion.gameObject, explosionPrefab.main.duration - 2.3f);
        onDestroy?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canMove) 
        {
            Target target = other.gameObject.GetComponent<Target>();
            if (target != null) target.TakeDamage();
            Destroy();
        }
    }
}
