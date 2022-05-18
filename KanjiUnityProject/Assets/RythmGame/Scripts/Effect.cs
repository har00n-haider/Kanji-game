using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Effect : MonoBehaviour
{
    [SerializeField]
    private VisualEffect visualEffect;
    [SerializeField]
    private string audioEffectName;
    private AudioClip audioEffectClip;
    [SerializeField]
    private float effectLifetime;


    private void Start()
    {
        audioEffectClip = GameManager.Instance.GameAudio.GetClip(audioEffectName);
        AudioSource.PlayClipAtPoint(audioEffectClip, transform.position);
        visualEffect.SendEvent("OnPlay");
        Destroy(gameObject, effectLifetime);
    }


}