using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Grunt : MonoBehaviour, IPromptHolderControllable
{
    // configuration
    public float speed = 0.1f;

    public bool canMove = true;
    public System.Action onDestroy;
    public PromptConfiguration promptConfig;

    // state
    private int health;

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

    public void SetHealth(int health)
    {
        this.health = health;
    }

    public void TakeDamage(int damage)
    {
        if (health > 0) health -= damage;
        if (health <= 0) Destroy();
    }

    public int MyProperty { get; set; }

    public Transform getTransform => transform;

    public bool isDestroyed => this == null;

    public PromptConfiguration getPromptConfig => promptConfig;
}