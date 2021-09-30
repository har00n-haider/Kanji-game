using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatArenaMode : MonoBehaviour
{
    // refs
    public GameObject deathText;
    public GameObject startButton;
    public MainCharacter mainCharacter;
    public EnemySpawner enemySpawner;

    // When start button is clicked
    public void OnStart()
    {
        startButton.SetActive(false);
        enemySpawner.StartSpawning();
    }


    public void Reset()
    {
        deathText.SetActive(false);
        startButton.SetActive(true);
        enemySpawner.ClearEnemies();
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
            Reset();
        }
    }
}
