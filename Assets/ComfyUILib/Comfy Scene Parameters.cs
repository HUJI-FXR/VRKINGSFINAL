using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComfySceneParameters : MonoBehaviour
{
    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;

    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;

    private string GameManagerScene = "Empty Scene";
    private static bool loadedGameManagerScene = false;

    // Scene-local objects
    public GameObject diffusables;
    public Gadget gadget;
    public AudioSource headAudioSource;

    public bool LoadComfyParametrs = true;

    private void Awake()
    {
        StartCoroutine(LoadGameManagerScene());
    }

    IEnumerator LoadGameManagerScene()
    {
        if (SceneManager.sceneCount < 3 && !loadedGameManagerScene)
        {
            loadedGameManagerScene = true;

            var asyncLoad = SceneManager.LoadSceneAsync(GameManagerScene, LoadSceneMode.Additive);            
            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            //asyncLoad.allowSceneActivation = false;
            Debug.Log("Got to part of script after load scene!");
        }
        else
        {
            loadedGameManagerScene = true;
        }

        if (!loadedGameManagerScene || LoadComfyParametrs)
        {
            GameManager.getInstance().InitiateSceneParameters(comfyOrganizer, comfySceneLibrary,
            radiusDiffusionTexture, uiDiffusionTexture, diffusables, gadget, headAudioSource);
        }
    }
}
