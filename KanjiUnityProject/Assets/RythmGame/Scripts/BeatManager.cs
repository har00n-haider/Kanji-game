using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Beat
{
    public enum BeatType
    {
        HalfBeat, // Eigth Note for 4 | 4
        Beat,     // Quarter Note for 4 | 4
        Bar       // Where the bar start/end is, i.e. after 4 Beats for 4 | 4
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


/// <summary>
/// Responsible for playing the songs and keep track of beats
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BeatManager : MonoBehaviour
{
    // configuration
    [SerializeField]
    private float bpm;
    public float BeatPeriod { get { return 60.0f / bpm; } }
    public float HalfBeatPeriod { get { return 30.0f / bpm; } }
    [SerializeField]
    private int timeSignatureHi;
    [SerializeField]
    private int timeSignatureLo;
    public int NumHalfBeatsPerBar { get { return timeSignatureHi * 2; }}
    [SerializeField]
    private float beatHitAllowance;
    public float BeatHitAllowance { get { return beatHitAllowance; } }
    // max delay that it might take to load the sample. this may involve opening
    // buffering a streamed file and should therefore take any worst-case delay into account.
    public double preloadTimeDelta;
    public bool enableMetronome;
    public float metronomeVolume;
    public int metronomeBaseBpmMultiple;

    // refs
    [SerializeField]
    private AudioClip songClip;
    private AudioSource audioSource;
    [SerializeField]
    private Metronome metronome;

    // state
    public bool IsSongPlaying { get { return running; } }
    public Beat NextBeat { get { return beatMap[nextBeatIntIdx];}}
    private double startTime;
    private bool running = false;


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

        if(enableMetronome)
        {
            metronome.Init(bpm * metronomeBaseBpmMultiple, 
                timeSignatureHi, 
                timeSignatureLo, 
                startTime, 
                metronomeVolume);
        }
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
        }
    }

    private void GenerateBeatMap() 
    {
        int noOfHalfBeatsInSong = (int)(songClip.length * (bpm * 60)) * 2; 
        for (int halfBeatCtr = 0; halfBeatCtr < noOfHalfBeatsInSong; halfBeatCtr++)
        {
            // we can only have mutually exclusive beat types at the moment
            if( halfBeatCtr % timeSignatureHi == 0)
            {
                beatMap.Add(new Beat(Beat.BeatType.Bar, startTime + halfBeatCtr * HalfBeatPeriod, halfBeatCtr));
            }
            else if (halfBeatCtr % 2 == 0)
            {
                beatMap.Add(new Beat(Beat.BeatType.Beat, startTime + halfBeatCtr * HalfBeatPeriod, halfBeatCtr));
            }
            else
            {
                beatMap.Add(new Beat(Beat.BeatType.HalfBeat, startTime + halfBeatCtr * HalfBeatPeriod, halfBeatCtr));
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
    public Beat GetNextHalfBeat(int beatsToSkip = 0, Beat referenceBeat = null) 
    {
        Beat b = referenceBeat == null ? beatMap[nextBeatIntIdx] : referenceBeat;
        int beatIdx = b.beatId;
        // skip if required
        while(beatsToSkip != 0)
        {
            // skip the required amount depending on type 
            beatIdx++;
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