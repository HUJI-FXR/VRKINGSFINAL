using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used to load the needed parameters and variables onto the GameManager
/// </summary>
public class ComfySceneParameters : MonoBehaviour
{
    // ComfyUI Library scripts
    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;

    // Global DiffusionTextureChangers
    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;

    // Scene inwhich the GameManager will live
    private string GameManagerScene = "Empty Scene";

    // Used to keep track of the loading of the GameManager scene
    private static bool loadedGameManagerScene = false;

    // Scene-local objects
    public GameObject diffusables;

    public Gadget gadget;

    // Global 2D audio source
    public AudioSource headAudioSource;

    // If false, won't load scene parameters onto the GameManager
    public bool LoadComfyParametrs = true;

    // Used to send empty DiffusionRequests to load models onto memory as fast as possible
    public DiffusionRequest diffusionRequest;
    public bool SendBeginningDiffusionRequest = false;

    public IEnumerator LoadGameManagerScene()
    {
        // Loading the GameManager scene
        if (SceneManager.sceneCount < 3 && !loadedGameManagerScene)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(GameManagerScene, LoadSceneMode.Additive);            
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return new WaitForSeconds(0.2f);
            }
            //asyncLoad.allowSceneActivation = false;
            Debug.Log("Got to part of script after load scene!");
        }

        // Finished loading GameManager scene
        loadedGameManagerScene = true;

        // Loading parameters onto the GameManager
        if (!loadedGameManagerScene || LoadComfyParametrs)
        {
            if (comfyOrganizer == null)
            {
                comfyOrganizer = gameObject.GetComponent<ComfyOrganizer>();
            }
            if (comfySceneLibrary == null)
            {
                comfySceneLibrary = gameObject.GetComponent<ComfySceneLibrary>();
            }

            // Sending the Beginning DiffusionRequest onwards
            if (SendBeginningDiffusionRequest) 
            {
                DiffusionRequest newDiffusionRequest = comfyOrganizer.copyDiffusionRequest(diffusionRequest);
                GameManager.getInstance().InitiateSceneParameters(comfyOrganizer, comfySceneLibrary,
                radiusDiffusionTexture, uiDiffusionTexture, diffusables, gadget, headAudioSource, newDiffusionRequest);
            }
            else
            {
                GameManager.getInstance().InitiateSceneParameters(comfyOrganizer, comfySceneLibrary,
                radiusDiffusionTexture, uiDiffusionTexture, diffusables, gadget, headAudioSource, null);
            }
        }

        yield return new WaitForSeconds(1f);
    }
}
