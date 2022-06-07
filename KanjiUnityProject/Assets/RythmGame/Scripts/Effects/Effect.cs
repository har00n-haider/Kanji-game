using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public abstract class CustomEffect : MonoBehaviour
{
    public abstract void Play();
}


public class Effect : MonoBehaviour
{
    [SerializeField]
    private VisualEffect visualEffect;
    [SerializeField]
    private string audioEffectName;
    private AudioClip audioEffectClip;
    [SerializeField]
    private float effectLifetime;
    [SerializeField]
    private float volume = 1.0f;
    [SerializeField]
    private CustomEffect customEffect = null;

    private void Start()
    {
        audioEffectClip = GameManager.Instance.GameAudio.GetClip(audioEffectName);
        AudioSource.PlayClipAtPoint(audioEffectClip, transform.position, volume);
        if(visualEffect != null) visualEffect.SendEvent("OnPlay");
        if(customEffect != null) customEffect.Play();

        Destroy(gameObject, effectLifetime);
    }
}