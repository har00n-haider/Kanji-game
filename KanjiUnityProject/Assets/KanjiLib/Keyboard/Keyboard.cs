using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    public enum KeyboardType 
    {
        Draw,
        FlickRomaji,
        FlickHiragana,
        FlickKatana,
    }

    // keyboard 
    [SerializeField]
    private KeyboardType type;
    private FlickLayout flickLayout;
    private Kanji2D drawInput;

    private void Awake()
    {
        
    }

    private void Start()
    {
        flickLayout = GetComponentInChildren<FlickLayout>();
        drawInput = GetComponentInChildren<Kanji2D>();
    }

    private void Update()
    {
        switch (type) 
        {
            case KeyboardType.Draw:
                flickLayout.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;
            case KeyboardType.FlickHiragana:
            case KeyboardType.FlickKatana:
            case KeyboardType.FlickRomaji:
                flickLayout.gameObject.SetActive(true);
                drawInput.gameObject.SetActive(false);
                break;
        }
    }


}


