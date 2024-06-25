using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComfySceneParameters : MonoBehaviour
{
    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;

    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;

    private string GameManagerScene = "Empty Scene";
    private bool loadedGameManagerScene = false;

    // Scene-local objects
    public GameObject diffusables;
    public Gadget gadget;
    public AudioSource headAudioSource;

    private void Awake()
    {
        // Current scene, DontDestroyOnLoad and the GameManager Scene
        if (SceneManager.sceneCount < 3)
        {
            if (loadedGameManagerScene)
            {
                return;
            }
            SceneManager.LoadScene(GameManagerScene, LoadSceneMode.Additive);

            Debug.Log("Got to part of script after load scene!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started Passing Parameters to game manager");
        GameManager.getInstance().InitiateSceneParameters(comfyOrganizer, comfySceneLibrary,
            radiusDiffusionTexture, uiDiffusionTexture, diffusables, gadget, headAudioSource);
    }
}
