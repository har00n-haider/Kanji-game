using System.Collections;
using UnityEngine;


/// <summary>
///  Place where the input is configured
/// </summary>
public class GameInput : MonoBehaviour
{

    public static Vector3 MousePosition()
    {
        return Input.mousePosition;
    }

    public static bool GetButton1Down()
    {
        return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
    }


    public static bool GetButton1Up()
    {
        return Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space);
    }

    public static bool GetButton1()
    {
        return Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
    }


}
