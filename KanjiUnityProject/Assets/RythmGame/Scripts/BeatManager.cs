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
        public int beatId; // also the index into beat array
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
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = songClip;

        // setting up first event time
        startTime = AudioSettings.dspTime + preloadTimeDelta;
        audioSource.PlayScheduled(startTime);
        GenerateBeatMap();
        running = true;
    }

    void Start()
    {
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
    

    public bool CheckIfOnBeat(double beatTimeStamp)
    {
        double delta = beatTimeStamp - AudioSettings.dspTime;
        bool  result = Mathf.Abs((float)delta) < beatHitAllowance;
        return result;
    }

    // relative to the next beat by default
    public Beat GetNextBeatTimeStamp(int beatsToSkip = 0, Beat.BeatType type = Beat.BeatType.Beat, Beat referenceBeat = null) 
    {
        Beat b = referenceBeat == null ? beatMap[nextBeatIntIdx] : referenceBeat;
        int beatIdx = b.beatId;
        // latch on to a the required type (bar is still a beat)
        if(type != Beat.BeatType.Beat)
        {
            while(b.type != type)
            {
                beatIdx++;
                b = beatMap[beatIdx];
            }
        }
        // skip if required
        while(beatsToSkip != 0)
        {
            // skipe the required amount depending on type 
            beatIdx += type == Beat.BeatType.Bar ? numBeatsPerBar : 1;
            b = beatMap[beatIdx];
            beatsToSkip--;
        }
        return b.Clone();
    }

    public float TimeToBeat(Beat beat)
    {
        return (float)(beat.timestamp - AudioSettings.dspTime);
    }

}