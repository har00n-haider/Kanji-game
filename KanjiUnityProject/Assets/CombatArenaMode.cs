using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatArenaMode : MonoBehaviour
{
    // config
    public int initialHealth;

    // refs
    public GameObject deathText;
    public GameObject startButton;
    public MainCharacter mainCharacter;
    public EnemySpawner enemySpawner;

    public void Start()
    {
        ResetState();
    }

    // When start button is clicked
    public void OnStart()
    {
        ResetState();

        startButton.SetActive(false);
        enemySpawner.StartSpawning();
    }


    public void ResetState()
    {
        mainCharacter.SetHealth(initialHealth);
        mainCharacter.SetMaxHealth(initialHealth);
        deathText.SetActive(false);
        startButton.SetActive(true);
        enemySpawner.ResetState();
    }

    // TODO: hook this up properly with a event on the main character?
    private void Update()
    {
        if (mainCharacter.IsDead()) 
        {
            deathText.SetActive(true);
        }

        if(mainCharacter.IsDead() && Input.GetMouseButtonDown(0)) 
        {
            ResetState();
        }
    }
}
