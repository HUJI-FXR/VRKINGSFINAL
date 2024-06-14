using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralGameScript : MonoBehaviour
{
    public static GeneralGameScript instance;
    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;
    public GameObject diffusables;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        if (comfyOrganizer == null) {
            Debug.LogError("Please add a Comfy Organizer to the General object");
        }
        if (comfySceneLibrary == null)
        {
            Debug.LogError("Please add a Comfy Scene Library to the General object");
        }
        if (diffusables == null)
        {
            Debug.LogError("Please add a parent GameObject for the diffusable GameObjects to the General object");
        }
    }
}
