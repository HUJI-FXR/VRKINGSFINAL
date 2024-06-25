using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGadgetSounds : MonoBehaviour
{
    public string GadgetAudioClipFolder = "Sounds/SFX/GadgetSounds";
    private GeneralGameLibraries.AudioClipsLibrary AudioClipsLibrary;

    private void Awake()
    {
        AudioClipsLibrary = new GeneralGameLibraries.AudioClipsLibrary(GadgetAudioClipFolder);
    }

    public void PlaySound(string sound)
    {
        GameManager.getInstance().headAudioSource.PlayOneShot(AudioClipsLibrary.AudioClips[sound]);
    }
}
