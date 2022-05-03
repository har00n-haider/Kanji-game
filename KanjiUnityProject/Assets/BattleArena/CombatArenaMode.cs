using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CombatArenaMode : MonoBehaviour
{
    // config
    public int initialHealth;

    // state 

    private float uimessageTime;
    public float uimessageTimeMax = 3;

    // unity refs
    public TextMeshProUGUI uiText;
    public GameObject startButton;
    public MainCharacter mainCharacter;
    public EnemySpawner enemySpawner;

    public void Start()
    {
        ResetState();
        startButton.GetComponentInChildren<TextMeshProUGUI>().text = Strings.start;
    }

    // When start button is clicked
    public void OnStart()
    {
        ResetState();

        startButton.SetActive(false);
        DisplayUIText(Strings.waveStart);
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

    // TODO: hook this up properly with an event on the main character?
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

        uimessageTime += Time.deltaTime;
        if (uimessageTime >= uimessageTimeMax) 
        {
            uiText.gameObject.SetActive(false);
        }

    }

    void DisplayUIText(string message, Color? color = null)
    {
        uimessageTime = 0;
        uiText.text = message;
        uiText.color = color.HasValue ? color.Value : Color.green ;
        uiText.gameObject.SetActive(true);
    }

}
