using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;


[RequireComponent (typeof(AudioSource))]
public class AudioReact : MonoBehaviour
{
    AudioSource audioSource;
    public float[] samples;

    public bool wentOverThreshold = false;

    private float avg = 0f;
    private float rollingAvg = 0f;

    [Range(0f, 1f)]
    private float rollingAvgAlpha = 0.9f;

    // Start is called before the first frame update
    void Start()
    {
        samples = new float[512];
        audioSource = GetComponent<AudioSource> ();
    }

    // Update is called once per frame
    void Update()
    {
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
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }
}
