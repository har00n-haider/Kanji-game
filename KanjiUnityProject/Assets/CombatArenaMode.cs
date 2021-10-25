using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatArenaMode : MonoBehaviour
{
    // config
    public int initialHealth;

    // refs
    public TextMeshProUGUI uiText;
    public GameObject startButton;
    public MainCharacter mainCharacter;
    public EnemySpawner enemySpawner;

    public void Start()
    {
        ResetState();

        // Initial text
        startButton.GetComponentInChildren<TextMeshProUGUI>().text = Strings.start;
        uiText.text = Strings.deathText;
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
        uiText.gameObject.SetActive(false);
        startButton.SetActive(true);
        enemySpawner.ResetState();
    }

    // TODO: hook this up properly with a event on the main character?
    private void Update()
    {
        if (mainCharacter.IsDead()) 
        {
            uiText.gameObject.SetActive(true);
        }

        if(mainCharacter.IsDead() && Input.GetMouseButtonDown(0)) 
        {
            ResetState();
        }
    }
}
