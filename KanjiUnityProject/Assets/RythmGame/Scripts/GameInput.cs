using System.Collections;
using UnityEngine;


/// <summary>
///  Place where the input is configured
/// </summary>
public class GameInput : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool GetButton1Down()
    {
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
    }


    public bool GetButton1Up()
    {
        return Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space);
    }

    public bool GetButton1()
    {
        return Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
    }


}
