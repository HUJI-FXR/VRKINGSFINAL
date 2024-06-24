using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComfySceneParameters : MonoBehaviour
{

    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;

    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;


    // Scene-local objects
    public GameObject diffusables;
    public Gadget gadget;
    public AudioSource headAudioSource;
// Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started Passing Parameters to game manager");
        GameManager.getInstance().InitiateSceneParameters(comfyOrganizer, comfySceneLibrary,
            radiusDiffusionTexture, uiDiffusionTexture, diffusables, gadget, headAudioSource);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
