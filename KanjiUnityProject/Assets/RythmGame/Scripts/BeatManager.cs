using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class BeatManager : MonoBehaviour
{
    public class Beat
    {
        public enum BeatType
        {
            Beat,
            Bar
        }

        public Beat(BeatType type, double timestamp, int beatId)
        {
            this.type = type;
            this.timestamp = timestamp;
            this.beatId = beatId;
        }

        public BeatType type;
        public double timestamp;
        public int beatId;
    }

    [SerializeField]
    private float bpm;
    [SerializeField]
    private int numBeatsPerBar;
    public int NumBeatsPerBar { get { return numBeatsPerBar; } }
    [SerializeField]
    private AudioClip songClip;
    [SerializeField]
    private float beatHitAllowance;


    public float BeatHitAllowance { get { return beatHitAllowance; } }
    public float BeatPeriod { get { return 60.0f / bpm; } }
    public bool IsSongPlaying { get { return running; } }
    public Beat NextBeat { get { return beatMap[nextBeatIntIdx];}}

    private AudioSource audioSource;
    private double startTime;
    private bool running = false;

    // max delay that it might take to load the sample. this may involve opening
    // buffering a streamed file and should therefore take any worst-case delay into account.
    private double preloadTimeDelta = 1; 

    private List<Beat> beatMap = new List<Beat>();
    private int nextBeatIntIdx = 0; // internal tracknig

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
        GenerateBeatMap();
        running = true;
    }

    void Update()
    {
        if (!running) return;

        UpdateNextBeat();
    }

    private void UpdateNextBeat() 
    {
        // update beat time
        if (AudioSettings.dspTime > NextBeat.timestamp)
        {
            nextBeatIntIdx++;
            //GameManager.Instance.ToggleColor();
        }
    }

    private void GenerateBeatMap() 
    {
        // generate entry for every beat
        // TODO: do for length of song
        for (int i = 0; i < 1000; i++)
        {
            if (i % 4 == 0)
            {
                beatMap.Add(new Beat(Beat.BeatType.Bar, startTime + i * BeatPeriod, i));
            }
            else
            {
                beatMap.Add(new Beat(Beat.BeatType.Beat, startTime + i * BeatPeriod, i));
            }
        }
    }
    

    public bool CheckIfOnBeat(double beatTimeStamp,  bool silent = true)
    {
        double delta = beatTimeStamp - AudioSettings.dspTime;
        bool  result = Mathf.Abs((float)delta) < beatHitAllowance;
        if(!silent) Debug.Log($"{delta:0.00} {(result ? "hit" : "miss")}");
        return result;
    }

    // relative to the next beat by default
    public Beat GetNextBarTimeStamp(int barsToSkip = 0, Beat referenceBeat = null) 
    {
        int nextBeatIdx = nextBeatIntIdx;

        Beat b = referenceBeat == null ? beatMap[nextBeatIdx] : referenceBeat;
        // latch on to a bar
        while(b.type != Beat.BeatType.Bar)
        {
            nextBeatIdx++;
            b = beatMap[nextBeatIdx];
        }
        // skip if required
        while(barsToSkip != 0)
        {
            nextBeatIdx += numBeatsPerBar;
            b = beatMap[nextBeatIdx];
            barsToSkip--;
        }
        return b.Clone();
    }

    // relative to the next beat by default
    public Beat GetNextBeatTimeStamp(int beatsToSkip = 0, Beat referenceBeat = null) 
    {
        int nextBeatIdx = nextBeatIntIdx;
        Beat b = referenceBeat == null ? beatMap[nextBeatIdx] : referenceBeat;
        // latch on to a beat
        while(b.type != Beat.BeatType.Beat)
        {
            nextBeatIdx++;
            b = beatMap[nextBeatIdx];
        }
        // skip if required
        while(beatsToSkip != 0)
        {
            nextBeatIdx++;
            b = beatMap[nextBeatIdx];
            beatsToSkip--;
        }
        return b.Clone();
    }

    public float TimeToBeat(Beat beat)
    {
        return (float)(beat.timestamp - AudioSettings.dspTime);
    }

}