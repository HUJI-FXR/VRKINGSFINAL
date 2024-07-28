using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComfyUILibraryConnector : MonoBehaviour
{
    const string ComfyUILibraryName = "ComfyUILib";

    public ComfyXROriginConnector comfyXROriginConnector;
    public GameObject diffusables;

    // Start is called before the first frame update
    void Start()
    {
        if (comfyXROriginConnector == null || diffusables == null)
        {
            Debug.LogError("Add the requirements to the Universal Comfy Object");
            return;
        }

        GameObject comfyUILibrary = GameObject.Find(ComfyUILibraryName);
        ComfySceneParameters curParameters = comfyUILibrary.AddComponent<ComfySceneParameters>();

        curParameters.gadget = comfyXROriginConnector.gadget;
        curParameters.uiDiffusionTexture = comfyXROriginConnector.uiDiffusionTexture;
        curParameters.radiusDiffusionTexture = comfyXROriginConnector.radiusDiffusionTexture;
        curParameters.headAudioSource = comfyXROriginConnector.headAudioSource;

        curParameters.diffusables = diffusables;

        StartCoroutine(curParameters.LoadGameManagerScene());
    }
}
