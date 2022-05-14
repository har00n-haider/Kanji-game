using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Effect : MonoBehaviour
{
    [SerializeField]
    private VisualEffect visualEffect;
    [SerializeField]
    private AudioClip audioEffectClip;
    [SerializeField]
    private float effectLifetime;


    private void Start()
    {

        visualEffect.SendEvent("OnPlay");


        AudioSource.PlayClipAtPoint(audioEffectClip, transform.position);


        Destroy(gameObject, effectLifetime);
    }


}