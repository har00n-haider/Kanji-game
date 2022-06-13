using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Beat
{
    public Beat(double timestamp, int beatId)
    {
        this.timestamp = timestamp;
        this.beatId = beatId;
    }
    public double timestamp;
    public int beatId;
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
    public int NumHalfBeatsPerBar { get { return timeSignatureHi * 2; } }
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
    private BeatManagerConfig config { get { return GameManager.Instance.Settings.beatManagerConfig; } } 

    // state
    public bool IsSongPlaying { get { return running; } }

    private double startTime;
    public double timeIntoSong { get { return AudioSettings.dspTime - startTime; } }
    private bool running = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = songClip;

        // setting up first event time
        startTime = AudioSettings.dspTime + preloadTimeDelta;
        audioSource.PlayScheduled(startTime);
        running = true;

        if (enableMetronome)
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
    }

    public bool CheckIfOnSongBeat()
    {
        return timeIntoSong % BeatPeriod < config.beatHitAllowance;
    }

    public bool CheckIfOnBeat(Beat beat)
    {
        double delta = beat.timestamp - timeIntoSong;
        bool result = Mathf.Abs((float)delta) < config.beatHitAllowance;
        return result;
    }

    // TODO: BRoken 
    // relative to the next beat by default
    public Beat GetNextHalfBeat(int beatsToSkip = 0, Beat referenceBeat = null)
    {
        Beat b = null;
        //Beat b = referenceBeat == null ? beatMap[nextBeatIntIdx] : referenceBeat;
        //int beatIdx = b.beatId;
        //// skip if required
        //while (beatsToSkip != 0)
        //{
        //    // skip the required amount depending on type 
        //    beatIdx++;
        //    b = beatMap[beatIdx];
        //    beatsToSkip--;
        //}
        return b.Clone();
    }

    public bool IsBeatWithinRange(Beat beat, float range)
    {
        double delta = beat.timestamp - timeIntoSong; 
        return delta < 0 ? false : delta < range; // false if missed in any case
    }

    public bool IsBeatMissed(Beat beat)
    {
        return timeIntoSong > beat.timestamp + config.beatHitAllowance;
    }

}