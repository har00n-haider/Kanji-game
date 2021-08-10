using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public int health = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage() 
    {
        if (health > 0) 
        { 
            --health; 
            Debug.Log(health + " health left!");
        }
        else 
        {
            Destroy(gameObject);
        }
    }

}
