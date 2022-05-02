using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    
public class MainCamera : MonoBehaviour
{
    // refs

    public GameObject mainCharacter;

    [Range(0, 100)]
    public float distanceToCharacter = 100f;

    public float pitch;
    public float yaw;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.position = 
            mainCharacter.transform.position + -transform.forward * distanceToCharacter;
    }
}
