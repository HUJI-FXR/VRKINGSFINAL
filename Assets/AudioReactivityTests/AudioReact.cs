using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent (typeof(AudioSource))]
public class AudioReact : MonoBehaviour
{
    AudioSource audioSource;
    public float[] samples = new float[512];

    public float sum = 0f;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource> ();
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource ();

        sum = samples.Sum();
        if (sum > 1 )
        {
            Debug.Log(samples.Sum());
        }
    }

    void GetSpectrumAudioSource()
    {
        audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }
}
