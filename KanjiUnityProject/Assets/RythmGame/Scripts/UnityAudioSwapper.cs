using UnityEngine;
using System.Collections;

// Basic demonstration of a music system that uses PlayScheduled to preload and sample-accurately
// stitch two AudioClips in an alternating fashion.  
[RequireComponent(typeof(AudioSource))]
public class UnityAudioSwapper : MonoBehaviour
{
    [SerializeField]
    private float bpm = 65.0f;
    [SerializeField]
    private int numBeatsPerSegment = 4;
    [SerializeField]
    private AudioClip[] clips = new AudioClip[2];
    [SerializeField]
    private float beatHitAllowance;


    public float BeatPeriodSec { get { return 60.0f / bpm; } }
    public bool IsSongPlaying { get { return running; } }

    private double nextEventTime;
    private int flip = 0;
    private AudioSource[] audioSources = new AudioSource[2];
    private bool running = false;
    // max delay that it might take to load the sample. this may involve opening
    // buffering a streamed file and should therefore take any worst-case delay into account.
    private double preloadTimeDelta = 1 ; 
    private double nextBeatTime;



    void Start()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject child = new GameObject("Player");
            child.transform.parent = gameObject.transform;
            audioSources[i] = child.AddComponent<AudioSource>();
        }
        // setting up first event time.
        nextEventTime = AudioSettings.dspTime + preloadTimeDelta +  0.5f;
        nextBeatTime = nextEventTime;
        running = true;
    }

    void Update()
    {
        if (!running) return;

        // update beat
        if (AudioSettings.dspTime > nextBeatTime)
        {
            //Debug.Log("beat");  
            nextBeatTime += BeatPeriodSec;
            GameManager.instance.ToggleColor(); 
        }

        // check if we have crossed the point where we should load the file
        double timeToNextEvent = nextEventTime - AudioSettings.dspTime;
        if (timeToNextEvent < preloadTimeDelta)
        {
            // if so schedule the next clip
            audioSources[flip].clip = clips[flip];
            audioSources[flip].PlayScheduled(nextEventTime);
            // Debug.Log($"Scheduled source {flip} to start at time {nextEventTime:F3} current time {AudioSettings.dspTime:F3}");

            float offsetToNextEvent = 60.0f * (numBeatsPerSegment / bpm);
            nextEventTime += offsetToNextEvent;

            // Flip between two audio sources so that the loading process of one does not interfere with the one that's playing out
            flip = 1 - flip;
        }
    }

    public bool CheckIfOnBeat()
    {
        float currentDelta = Mathf.Abs((float)(nextBeatTime - AudioSettings.dspTime));
        bool  result = currentDelta < beatHitAllowance;
        //Debug.Log(result + " " + currentDelta + "  " + beatHitAllowance);

        return result;
    }


}