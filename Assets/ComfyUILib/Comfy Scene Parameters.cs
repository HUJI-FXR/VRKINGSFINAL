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

    public IEnumerator LoadGameManagerScene()
    {
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

        loadedGameManagerScene = true;

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

            GameManager.getInstance().InitiateSceneParameters(comfyOrganizer, comfySceneLibrary,
            radiusDiffusionTexture, uiDiffusionTexture, diffusables, gadget, headAudioSource);
        }

        yield return new WaitForSeconds(1f);
    }
}
