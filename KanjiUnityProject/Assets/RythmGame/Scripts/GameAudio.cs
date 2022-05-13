using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;


public class BeatMap 
{

    [System.Serializable]
    public class Entry 
    {
        public int time;
        public int type;
    }

    [System.Serializable]
    private class EntryList 
    {
        public Entry[] list;
    }

    public static Entry[] LoadBeatMap()
    {
        EntryList el = new EntryList();
        using (StreamReader sr = new StreamReader("Assets/StreamingAssets/fire_work_drums_midi.json"))
        {
            string content = sr.ReadToEnd();
            //HACK: as unity json utility does not allow a top level array object
            string jsonString = "{ \"list\": " + content + "}";
            el = JsonUtility.FromJson<EntryList>(jsonString);
        }
        return el.list;
    }

}


public class GameAudio : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSourceGameMusic;

    [SerializeField]
    private AudioSource _audioSourceGameEffects;

    [SerializeField]
    private AudioSource _audioSourceMetronome;

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

    public float SongTime { get { return songTime; } }
    float songTime = 0;

    public float NextBeatTime { get { return nextBeatTime; } }
    float nextBeatTime = 0;

    // beat management stuff
    public float metronomeOffsetSeconds = 0;
    public bool metronomeAudioEnable;
    public float beatMapOffsetSeconds = 0;
    public float beatClickThreshold;
    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    public float bpm = 0;
    public float beatPeriod { get { return 360 / bpm; } }
    public int numBeatsPerSegment = 1;
    private double nextEventTime;
    private float currentDelta;


    // Awake() is called before Start.
    void Awake()
    {
        Assert.IsNotNull(_audioSourceGameMusic);
        Assert.IsNotNull(_audioSourceGameEffects);
        Assert.IsNotNull(_audioSourceMetronome);

        //Assert.IsNotNull(_audioClipGameMusic);
        //Assert.IsNotNull(_audioClipLaunchFirework);
        //Assert.IsNotNull(_audioClipFireworkExplode);

        SubscribeToAppEvents();
    }

    // Start is called before the first frame update
    void Start()
    {
        _audioSourceMetronome.clip = _audioClipMetronome;

        nextEventTime = AudioSettings.dspTime + 2.0f;
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
        UpdateBeatTime();
    }

    private void UpdateBeatTime() 
    {

        double time = AudioSettings.dspTime;

        currentDelta += Time.deltaTime;
        if (time + 1.0f + metronomeOffsetSeconds > nextEventTime)
        {

            if (!_audioSourceGameMusic.isPlaying) 
            {
            
                _audioSourceGameMusic.clip = _audioClipGameMusic;
                _audioSourceGameMusic.loop = false;
                _audioSourceGameMusic.volume = 0.5f;
                _audioSourceGameMusic.PlayScheduled(nextEventTime);
            }

            // Place the next event 1 beat from here at a rate of 140 beats per minute
            nextEventTime += 60.0f / bpm * numBeatsPerSegment;

            if(metronomeAudioEnable) _audioSourceMetronome.Play();

            currentDelta = 0;
        }
    }

    public bool CheckIfOnBeat() 
    {
        return currentDelta < beatClickThreshold || currentDelta > (beatPeriod - beatClickThreshold);  
    }

    public bool IsSongPlaying()
    {
        return _audioSourceGameMusic.isPlaying;
    }

    /// <summary>
    /// Subscribe to various AppEvents which may trigger or cancel sound effects or music.
    /// </summary>
    private void SubscribeToAppEvents()
    {
        //AppEvents.OnStartLevel += HandleStartLevel;
        AppEvents.OnEndLevel += HandleEndLevel;
        AppEvents.OnSpawnFirework += HandleSpawnFirework;
        AppEvents.OnSpawnExplosion += HandleSpawnExplosion;
        AppEvents.OnFireworkMissed += HandleMissedFirework;
    }

    /// <summary>
    /// Unsubscribe to all of the AppEvents which were subscribed to in SubscribeToAppEvents().
    /// </summary>
    private void UnsubscribeToAppEvents()
    {
        //AppEvents.OnStartLevel -= HandleStartLevel;
        AppEvents.OnEndLevel -= HandleEndLevel;
        AppEvents.OnSpawnFirework -= HandleSpawnFirework;
        AppEvents.OnSpawnExplosion -= HandleSpawnExplosion;
        AppEvents.OnFireworkMissed -= HandleMissedFirework;

    }

    private void HandleStartLevel()
    {
        _audioSourceGameMusic.clip = _audioClipGameMusic;
        _audioSourceGameMusic.loop = true;
        _audioSourceGameMusic.volume = 0.5f;
        _audioSourceGameMusic.Play();
        stopwatch.Start();
    }

    private void HandleEndLevel()
    {
        _audioSourceGameMusic.Stop();
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
