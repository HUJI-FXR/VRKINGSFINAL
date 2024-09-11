using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;


// TODO documentation

[RequireComponent (typeof(AudioSource))]
public class AudioReact : MonoBehaviour
{
    public AudioSource audioSource;

    [NonSerialized]
    public float[] samples = new float[512];

    [NonSerialized]
    public bool wentOverThreshold = false;

    private float avg = 0f;
    private float rollingAvg = 0f;

    [Range(0f, 1f)]
    private float rollingAvgAlpha = 0.9f;

    // Update is called once per frame
    void Update()
    {
        if (audioSource == null) return;

        GetSpectrumAudioSource ();
        avg = samples.Average();

        rollingAvg = rollingAvgAlpha * rollingAvg + (1 - rollingAvgAlpha) * avg;

        if (rollingAvg > 1.5 * avg)
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
