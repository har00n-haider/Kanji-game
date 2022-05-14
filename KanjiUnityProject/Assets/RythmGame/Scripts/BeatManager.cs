using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class BeatManager : MonoBehaviour
{
    [SerializeField]
    private float bpm;
    [SerializeField]
    private int numBeatsPerSegment;
    [SerializeField]
    private AudioClip songClip;
    [SerializeField]
    private float beatHitAllowance;


    public float BeatHitAllowance { get { return beatHitAllowance; } }
    public float BeatPeriod { get { return 60.0f / bpm; } }
    public bool IsSongPlaying { get { return running; } }

    private AudioSource audioSource;
    private double startTime;
    private bool running = false;

    // max delay that it might take to load the sample. this may involve opening
    // buffering a streamed file and should therefore take any worst-case delay into account.
    private double preloadTimeDelta = 1; 
    private double nextBeatTime;

    private List<double> beatMap = new List<double>();
    private int nextBeatIdx = 0;

    private void Awake()
    {
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = songClip;

        // setting up first event time.
        startTime = AudioSettings.dspTime + preloadTimeDelta;
        audioSource.PlayScheduled(startTime);
        nextBeatTime = startTime;
        running = true;
        GenerateBeatMap();
    }

    void Update()
    {
        if (!running) return;

        UpdateNextBeat();
    }

    private void UpdateNextBeat() 
    {
        // update beat time
        if (AudioSettings.dspTime > nextBeatTime)
        {
            nextBeatTime += BeatPeriod;

            GameManager.Instance.ToggleColor();
        }
    }

    private void GenerateBeatMap() 
    {
        float offset = 1 * BeatPeriod * numBeatsPerSegment; // skip this many
        // generate a beat entry every bar
        for (int i = 0; i < 20; i++)
        {
            beatMap.Add(startTime + offset + i * BeatPeriod * numBeatsPerSegment);
        }
    }

    public bool CheckIfOnBeat(double beatTimeStamp,  bool silent = true)
    {
        double delta = beatTimeStamp - AudioSettings.dspTime;
        bool  result = Mathf.Abs((float)delta) < beatHitAllowance;
        if(!silent) Debug.Log($"{delta:0.00} {(result ? "hit" : "miss")}");
        return result;
    }

    public double GetNextBeatTimeStamp() 
    {
        double beatTimeStamp = -1;
        if(nextBeatIdx < beatMap.Count) 
        {
            beatTimeStamp = beatMap[nextBeatIdx];
            nextBeatIdx++;
        }
        return beatTimeStamp;
    }


}