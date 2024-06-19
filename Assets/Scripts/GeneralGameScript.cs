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
    public Gadget gadget;
    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;

    [NonSerialized]
    public List<GameObject> diffusionList = new List<GameObject>();

    private void Awake()
    {
        if (instance != null)
        {
            return;
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
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
        if (gadget == null)
        {
            Debug.LogError("Please add a Gadget to the General object");
        }

        // todo maybe make these texturechangers into a universal and individual category?
        if (radiusDiffusionTexture == null)
        {
            // todo Maybe not send this one in other than last scene?
            Debug.LogError("Please add a RadiusDiffusionTexture to the General object");
        }
        if (uiDiffusionTexture == null)
        {
            Debug.LogError("Please add a UIDiffusionTexture to the General object");
        }

        foreach (Transform diffusionTransform in diffusables.transform)
        {
            diffusionList.Add(diffusionTransform.gameObject);
        }
    }
}
