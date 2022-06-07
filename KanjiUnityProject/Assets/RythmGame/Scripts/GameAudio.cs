using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;


public class GameAudio : MonoBehaviour
{

    public BeatManager BeatManager;

    // audio clips
    private Dictionary<string, AudioClip> audioClipMap = new Dictionary<string, AudioClip>();
    public AudioClip fail;
    public AudioClip select;
    public AudioClip success;
    public AudioClip keyPointCollect;
    
    void Awake()
    {
        SubscribeToAppEvents();
        BeatManager = GetComponentInChildren<BeatManager>();
    }

    void Start()
    {
        // set up the map
        audioClipMap["success"] = success;
        audioClipMap["fail"] = fail;
        audioClipMap["selected"] = select;
        audioClipMap["keyPointCollect"] = keyPointCollect;
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

    public AudioClip GetClip(string name)
    {
        return audioClipMap[name];
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


}
