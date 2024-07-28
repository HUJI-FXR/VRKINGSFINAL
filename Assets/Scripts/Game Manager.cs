using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Non scene-local objects
    private static GameManager instance = null;

    [NonSerialized]
    public string IP = "127.0.0.1:8188"; // jonathanmiroshnik-networks-24172136.thinkdiffusion.xyz

    [NonSerialized]
    public List<GameObject> diffusionList;

    // Scene-local objects
    [NonSerialized]
    public GameObject diffusables;
    [NonSerialized]
    public Gadget gadget;
    //[NonSerialized]
    public AudioSource headAudioSource;
    [NonSerialized]
    public ComfyOrganizer comfyOrganizer;
    [NonSerialized]
    public ComfySceneLibrary comfySceneLibrary;
    [NonSerialized]
    public RadiusDiffusionTexture radiusDiffusionTexture;
    [NonSerialized]
    public UIDiffusionTexture uiDiffusionTexture;

    /*public string firstScene = "StartScene";
    public static bool loadFirstScene = true;*/

    private void Awake()
    {
        diffusionList = new List<GameObject>();
        if (instance != null)
        {
            return;
        }
        instance = this;
    }

    public static GameManager getInstance()
    {
        if (instance == null)
        {
            Debug.LogError("Awake the GameManager");
            //GameManager.instance.Awake();
            /*instance = new GameManager();
            instance.diffusionList = new List<GameObject>();*/
        }
        
        return instance;
    }

    public void LoadNextScene(string thisScene, string nextScene)
    {
        StartCoroutine(LoadScene(thisScene, nextScene));
    }

    public void InitiateSceneParameters(ComfyOrganizer _comfyOrganizer, ComfySceneLibrary _comfySceneLibrary, 
        RadiusDiffusionTexture _radiusDiffusionTexture, UIDiffusionTexture _uiDiffusionTexture, 
        GameObject _diffusables, Gadget _gadget, AudioSource _headAudioSource)
    {
        comfyOrganizer = _comfyOrganizer;
        comfySceneLibrary = _comfySceneLibrary;
        radiusDiffusionTexture = _radiusDiffusionTexture;
        uiDiffusionTexture = _uiDiffusionTexture;
        diffusables = _diffusables;
        gadget = _gadget;
        headAudioSource = _headAudioSource;
        
        if (diffusables == null)
        {
            Debug.LogError("Please add a parent GameObject for the diffusable GameObjects to the GameManager");
        }
        if (gadget == null)
        {
            Debug.LogError("Please add a Gadget to the GameManager");
        }
        
        if (headAudioSource == null)
        {
            Debug.LogError("Please add a Audio Source to the GameManager");
        } 
        
        if (uiDiffusionTexture.PopupDisplay == null || uiDiffusionTexture.displayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
        }

        if (uiDiffusionTexture.playGadgetSounds == null)
        {
            Debug.LogError("Add all UIDiffusionTexture inputs");
        }
        
        if (comfyOrganizer == null)
        {
            Debug.LogError("Please add a Comfy Organizer to the GameManager");
        }
        if (comfySceneLibrary == null)
        {
            Debug.LogError("Please add a Comfy Scene Library to the GameManager");
        }

        // todo maybe make these texturechangers into a universal and individual category?
        /*if (radiusDiffusionTexture == null)
        {
            // todo Maybe not send this one in other than last scene?
            Debug.LogError("Please add a RadiusDiffusionTexture to the GameManager");
        }*/
        
        
        if (uiDiffusionTexture == null)
        {
            Debug.LogError("Please add a UIDiffusionTexture to the GameManager");
        }
        
        
        foreach (Transform diffusionTransform in diffusables.transform)
        {
            diffusionList.Add(diffusionTransform.gameObject);
        }
    }



    // Use a courotine so u dont freeze the ui
    public IEnumerator LoadScene(string thisScene, string nextScene)
    {        
        // Only when the scene is loaded we can unload the orginally active screen
        var asyncUnload = SceneManager.UnloadSceneAsync(thisScene);

        while (!asyncUnload.isDone)
        {
            if (asyncUnload.progress >= 0.9f)
            {
                UnityEngine.Debug.Log("Unloading...");
                break;
            }

            yield return null;
        }


        // Load a scene in additive mode, meaning it wont unload the currently loaded scene if there is one
        var loadScene = SceneManager.LoadSceneAsync(
        nextScene,
        LoadSceneMode.Additive
        );

        loadScene.allowSceneActivation = false;

        // wait for the scene to load
        while (!loadScene.isDone)
        {
            if (loadScene.progress >= 0.9f)
            {
                break;
            }

            yield return null;
        }

        loadScene.allowSceneActivation = true;

        // HERE IS WHERE ALOT OF PEOPLE FAIL.
        // You need to wait for the scene to be loaded before you can unload the another scene.
        // This is because UNITY WILL NOT UNLOAD A scene if its the only one  currently active
        // If you look up, we allow scene activate only at 90% of the loading(for reasons im not sure but whatever), 
        // so now we have to wait for the rest of the 10% to load
        yield return null;

        bool d = SceneManager.SetActiveScene(SceneManager.GetSceneByName(nextScene));
        Debug.Log(SceneManager.GetActiveScene().name);

        comfySceneLibrary.StartComfySceneLibrary();
    }


}
