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

    public float avg = 0f;

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

        //Debug.Log(avg);
        /*if (avg > 1 )
        {
            Debug.Log(avg);
        }*/
    }

    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }
}
