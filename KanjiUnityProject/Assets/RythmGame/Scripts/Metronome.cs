using UnityEngine;
using System.Collections;


/// <summary>
/// All times are in seconds unless otherwise stated
/// 
/// The code example shows how to implement a metronome that procedurally
/// generates the click sounds via the OnAudioFilterRead callback.
/// While the game is paused or the suspended, this time will not
/// be updated and sounds playing will be paused. Therefore developers
/// of music scheduling routines do not have to do any rescheduling after the app is unpaused
/// 
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Metronome : MonoBehaviour
{
    private double bpm;
    private int timeSignatureHi;
    private int timeSignatureLo;
    private float gain ;
    private double nextTick = 0.0F;
    private float amp = 0.0F;
    private float phase = 0.0F;
    private double sampleRate = 0.0F;
    private int accent;
    private bool running = false;

    public void Init(double bpm, int timeSignatureHi, int timeSignatureLo, double startTick, float gain = 0.5f)
    {
        this.bpm = bpm;
        this.timeSignatureHi = timeSignatureHi;
        this.timeSignatureLo = timeSignatureLo;
        this.gain = gain;

        accent = timeSignatureHi;
        sampleRate = AudioSettings.outputSampleRate; // Hz
        nextTick = startTick * sampleRate; // 
        running = true;
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