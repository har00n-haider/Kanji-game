using UnityEngine;
using System.Collections;

// The code example shows how to implement a metronome that procedurally
// generates the click sounds via the OnAudioFilterRead callback.
// While the game is paused or the suspended, this time will not
// be updated and sounds playing will be paused. Therefore developers of music scheduling routines do not have to do any rescheduling after the app is unpaused

[RequireComponent(typeof(AudioSource))]
public class UnityMetronome : MonoBehaviour
{
    public double bpm = 65.0F;
    public float gain = 0.5F;
    public int timeSignatureHi = 4;
    public int timeSignatureLo = 4;
    private double nextTick = 0.0F;
    private float amp = 0.0F;
    private float phase = 0.0F;
    private double sampleRate = 0.0F;
    private int accent;
    private bool running = false;

    void Start()
    {
        accent = timeSignatureHi;
        double startTick = AudioSettings.dspTime; // seconds
        sampleRate = AudioSettings.outputSampleRate; // Hz
        nextTick = startTick * sampleRate; // 
        running = true;
    }


    private void Update()
    {
    }


    // precudurally generating audio on a periodic basis
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!running)
            return;

        double samplesPerTick = sampleRate * 60.0F / bpm * 4.0F / timeSignatureLo;
        double sample = AudioSettings.dspTime * sampleRate;
        int dataLen = data.Length / channels; // e.g. 2 channel [c1][c2][c1][c2] ... 

        for (int n = 0; n < dataLen; n++)
        {
            // generate volume data based on a sin wave
            float audioSample = gain * amp * Mathf.Sin(phase);
            // put the same data in all channels
            int i = 0;
            while (i < channels)
            {
                data[n * channels + i] += audioSample; 
                i++;
            }
            // prep internal values for the next sample
            while (sample + n >= nextTick)
            {
                nextTick += samplesPerTick;
                amp = 1.0F;
                // if its the high beat, then amplify it
                if (++accent > timeSignatureHi)
                {
                    accent = 1;
                    amp *= 2.0F;
                }
                Debug.Log("Tick: " + accent + "/" + timeSignatureHi);
            }
            phase += amp * 0.3F; //??
            amp *= 0.993F; // ??

        }
    }
}