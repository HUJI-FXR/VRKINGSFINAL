using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Non scene-local objects
    private static GameManager instance;

    [SerializeField] private string firstScene;


    [NonSerialized]
    public List<GameObject> diffusionList;
    
    // Scene-local objects
    public GameObject diffusables;
    public Gadget gadget;
    public AudioSource headAudioSource;
    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;
    
    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;

    

    private void Awake()
    {
        diffusionList = new List<GameObject>();
        if (instance != null)
        {
            return;
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
        
        
        
        SceneManager.LoadScene(firstScene, LoadSceneMode.Additive);
        
        Debug.Log("Got to part of script after load scene!");
        
        
    }    

    public static GameManager getInstance()
    {
        if (instance == null)
        {
            instance = new GameManager();
        }

        return instance;
    }

    public void LoadNextScene(string thisScene, string nextScene)
    {
        SceneManager.UnloadSceneAsync(thisScene);
        SceneManager.LoadScene(nextScene, LoadSceneMode.Additive);
    }
    
    /*

    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;

    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;


    // Scene-local objects
    public GameObject diffusables;
    public Gadget gadget;
    public AudioSource headAudioSource;
    */

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
    
    


}
