using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grunt : MonoBehaviour, IKankiTraceable
{
    public float speed = 0.1f;

    public bool canMove = true;
    public System.Action onDestroy;

    // unity references
    [SerializeField]
    private AudioClip explosionSound;

    [SerializeField]
    private AudioClip ricochetSound;

    [SerializeField]
    private ParticleSystem explosionPrefab;

    private GameObject mainCharacter = null;

    private void Awake()
    {
        mainCharacter = GameObject.FindGameObjectWithTag("MainCharacter");
    }

    // Update is called once per frame
    private void Update()
    {
        if (canMove) Move();
    }

    private void Move()
    {
        gameObject.transform.LookAt(mainCharacter.transform);
        gameObject.transform.position += gameObject.transform.forward * speed * Time.deltaTime;
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
        Destroy(explosion.gameObject, explosionPrefab.main.duration - 2.4f);
        onDestroy?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canMove)
        {
            if (other.transform == mainCharacter.transform)
            {
                Destroy();
            }
        }
    }
}