using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Keyboard : MonoBehaviour
{
    public enum Type 
    {
        Draw,
        FlickRomaji,
        FlickHiragana,
        FlickKatana,
    }

    // keyboard 
    [SerializeField]
    private Type type;

    // refs
    [SerializeField]
    private FlickLayout hiraganaFlickInput;
    [SerializeField]
    private FlickLayout katakanaFlickInput;
    [SerializeField]
    private FlickLayout romajiFlickInput;
    [SerializeField]
    private Kanji2D drawInput;

    private void Awake()
    {
        
    }

    private void Start()
    {
    }

    private void Update()
    {
        switch (type) 
        {
            case Type.Draw:
                hiraganaFlickInput.gameObject.SetActive(false);
                drawInput.gameObject.SetActive(true);
                break;
            case Type.FlickHiragana:
            case Type.FlickKatana:
            case Type.FlickRomaji:
                hiraganaFlickInput.gameObject.SetActive(true);
                drawInput.gameObject.SetActive(false);
                break;
        }
    }


}


