using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;


public class GameAudio : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSourceGameEffects;

    [SerializeField]
    [Tooltip("Music which is played during the Game")]
    private AudioClip _audioClipGameMusic;

    [SerializeField]
    [Tooltip("Sfx for when a Firework is launched")]
    private AudioClip _audioClipLaunchFirework;

    [SerializeField]
    [Tooltip("Sfx for when a Firework is exploded")]
    private AudioClip _audioClipFireworkExplode;

    [SerializeField]
    [Tooltip("Sfx for when a Firework fizzles out (miss)")]
    private AudioClip _audioClipFizzle;

    [SerializeField]
    private AudioClip _audioClipMetronome;

    public BeatManager BeatManager;

    void Awake()
    {
        Assert.IsNotNull(_audioSourceGameEffects);
        SubscribeToAppEvents();
        BeatManager = GetComponentInChildren<BeatManager>();
    }

    void Start()
    {

    }

    /// <summary>
    /// On scene closing.
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeToAppEvents();
    }


    private void Update()
    {

    }




    /// <summary>
    /// Subscribe to various AppEvents which may trigger or cancel sound effects or music.
    /// </summary>
    private void SubscribeToAppEvents()
    {
    }

    /// <summary>
    /// Unsubscribe to all of the AppEvents which were subscribed to in SubscribeToAppEvents().
    /// </summary>
    private void UnsubscribeToAppEvents()
    {

    }

    private void HandleStartLevel()
    {

    }

    private void HandleEndLevel()
    {
    }

    private void HandleSpawnFirework(GameObject fireworkInstance, Vector3 fireworkSpawnForce)
    {
        // Play 'firework launch' sound fx from this source.
        AudioSource _audioSource = fireworkInstance.GetComponent<AudioSource>();
        _audioSource.clip = _audioClipLaunchFirework;
        _audioSource.Play();
    }

    private void HandleSpawnExplosion(GameObject explosionInstance)
    {
        // Play 'firework explode' sound fx from this source.
        AudioSource _audioSource = explosionInstance.GetComponent<AudioSource>();
        _audioSource.clip = _audioClipFireworkExplode;
        _audioSource.Play();
    }

    private void HandleMissedFirework(GameObject fireworkInstance)
    {
        // HACK: should have a missed effect, same as explode
        AudioSource.PlayClipAtPoint(_audioClipFizzle, fireworkInstance.transform.position);
    }

}
