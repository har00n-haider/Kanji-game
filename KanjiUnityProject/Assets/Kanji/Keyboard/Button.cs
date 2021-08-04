using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(TextMeshProUGUI))]
public class Button : MonoBehaviour
{

    TextMeshProUGUI textMesh;
    [SerializeField]
    private char buttonChar;

    // Start is called before the first frame update
    void Start()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        textMesh.text = buttonChar.ToString();
        gameObject.name = "Button " + textMesh.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
