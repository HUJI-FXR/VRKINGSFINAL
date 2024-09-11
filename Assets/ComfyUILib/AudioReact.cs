using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using static Unity.Burst.Intrinsics.X86;


// TODO documentation

[RequireComponent (typeof(AudioSource))]
public class AudioReact : MonoBehaviour
{
    public AudioSource audioSource;

    private const int NUM_CHANNELS = 512;

    [NonSerialized]
    public float[] samples = new float[NUM_CHANNELS];

    [NonSerialized]
    public bool wentOverThreshold = false;

    private float avg = 0f;
    private float rollingAvg = 0f;

    [Range(0, NUM_CHANNELS - 1)]
    public int MaximalChannel;
    [Range(0, NUM_CHANNELS-1)]
    public int MinimalChannel;    

    [Range(0f, 1f)]
    public float rollingAvgAlpha = 0.5f;

    private void OnValidate()
    {
        if (MinimalChannel > MaximalChannel)
        {
            Debug.LogError("The Minimal Channel cannot be higher than the Maximal Channel");
            MinimalChannel = MaximalChannel;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (audioSource == null) return;

        GetSpectrumAudioSource ();
        var avgSamples = samples.Skip(MinimalChannel).Take(MaximalChannel - MinimalChannel).ToArray();
        avg = avgSamples.Average();

        rollingAvg = rollingAvgAlpha * rollingAvg + (1 - rollingAvgAlpha) * avg;

        if (rollingAvg > 1.2 * avg)
        {
            wentOverThreshold = true;
        }
        else
        {
            wentOverThreshold = false;
        }
    }

    void GetSpectrumAudioSource()
    {
        if (audioSource == null) return;

        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }
}
