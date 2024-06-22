using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static GameManager instance;

    public ComfyOrganizer comfyOrganizer;
    public ComfySceneLibrary comfySceneLibrary;
    public GameObject diffusables;
    public Gadget gadget;
    public RadiusDiffusionTexture radiusDiffusionTexture;
    public UIDiffusionTexture uiDiffusionTexture;
    public AudioSource headAudioSource;

    [NonSerialized]
    public List<GameObject> diffusionList;

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
        if (comfyOrganizer == null)
        {
            Debug.LogError("Please add a Comfy Organizer to the GameManager");
        }
        if (comfySceneLibrary == null)
        {
            Debug.LogError("Please add a Comfy Scene Library to the GameManager");
        }
        if (diffusables == null)
        {
            Debug.LogError("Please add a parent GameObject for the diffusable GameObjects to the GameManager");
        }
        if (gadget == null)
        {
            Debug.LogError("Please add a Gadget to the GameManager");
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
        if (headAudioSource == null)
        {
            Debug.LogError("Please add a Audio Source to the GameManager");
        }

        foreach (Transform diffusionTransform in diffusables.transform)
        {
            diffusionList.Add(diffusionTransform.gameObject);
        }
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
}
