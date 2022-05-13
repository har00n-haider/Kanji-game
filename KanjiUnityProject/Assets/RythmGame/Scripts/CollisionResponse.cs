using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CollisionResponse : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        //if (collision.collider.tag != null)
        //{
        //    Debug.Log($"OnCollisionEnter(): this.GO.name = {this.name}, collision.collider.tag = {collision.collider.tag}");
        //}
        //else
        //{
        //    Debug.Log($"OnCollisionEnter(): this.GO.name = {this.name}, collision.collider.tag = null");
        //}

        // Collision.collider is the collider we hit. (See https://docs.unity3d.com/ScriptReference/Collision.html)
        if (collision.collider.CompareTag("KnightGround"))
        {
            // Debug.Log($"OnCollisionEnter(): ************* KnightGround");

            // Can destroy this game object, removes the cube, but that removes the audio source & the sound-effect stops.
            // Not a problem, can fix this.
            GameObject thisGameObject = this.gameObject;
            Destroy(thisGameObject);
        }
    }
}
