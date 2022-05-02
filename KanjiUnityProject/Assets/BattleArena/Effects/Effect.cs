using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: turn this into a regular class
public class Effect : MonoBehaviour
{
    [Header("Effects configuration")]
    public ParticleSystem particleEffectPrefab;

    public float particleSystemTime;
    public AudioClip audioEffectPrefab;

    public string effectName;

    public void StartEffect(Transform transform)
    {
        if(audioEffectPrefab != null)
        {
            AudioSource.PlayClipAtPoint(audioEffectPrefab, transform.position);
        }
        if(particleEffectPrefab !=  null)
        {
            ParticleSystem explosion = Instantiate(
                particleEffectPrefab,
                transform.position,
                transform.rotation);
            Destroy(explosion.gameObject, particleSystemTime);
        }
    }
}