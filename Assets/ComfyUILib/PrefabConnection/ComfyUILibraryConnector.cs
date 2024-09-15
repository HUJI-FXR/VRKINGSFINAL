using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComfyUILibraryConnector : MonoBehaviour
{
    const string ComfyUILibraryName = "ComfyUILib";

    public ComfyXROriginConnector comfyXROriginConnector;
    public GameObject diffusables;

    public ComfySceneParameters curParameters;    

    // Start is called before the first frame update
    void Start()
    {
        if (comfyXROriginConnector == null || diffusables == null)
        {
            Debug.LogError("Add the requirements to the Universal Comfy Object");
            return;
        }

        if (curParameters == null)
        {
            Debug.LogError("Add a Comfy Scene Parameters component to the ComfyUILib Object");
            return;
        }

        curParameters.gadget = comfyXROriginConnector.gadget;
        curParameters.uiDiffusionTexture = comfyXROriginConnector.uiDiffusionTexture;
        curParameters.radiusDiffusionTexture = comfyXROriginConnector.radiusDiffusionTexture;
        curParameters.diffusables = diffusables;

        // Indicates to the parameters object that the connector has loaded all the relevant parameters into it
        curParameters.LoadedConnectorParameters = true;
    }
}
