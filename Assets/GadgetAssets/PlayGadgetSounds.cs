using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGadgetSounds : MonoBehaviour
{
    public AudioSource audioSource;
    public List<AudioClip> sounds;

    private void Start()
    {
        if (sounds == null || audioSource == null || sounds.Count != 3) {
            Debug.LogError("Add everything needed to the PlayGadgetSounds");
        }
    }
    public void PlaySound(string sound)
    {
        switch (sound)
        {
            case "HoverOverElements":
                audioSource.PlayOneShot(sounds[0], 0.2f);
                break;
            case "SelectElement":
                audioSource.PlayOneShot(sounds[1], 1);
                break;
            case "ShowUIElement":
                audioSource.PlayOneShot(sounds[2], 1);
                break;
        }
    }
}
