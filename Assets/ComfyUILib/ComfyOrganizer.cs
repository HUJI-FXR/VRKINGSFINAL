using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;
using System.Net;
using UnityEngine.Rendering;


// TODO maybe remove requestNum from this class?

/// <summary>
/// Describes a single Diffusion Image generation Request which goes throughout the system, picking up the relevant
/// information for the Request, sends it to the generation, and sends the images to the proper targets, to be finished.
/// </summary>
[Serializable]
public class DiffusionRequest
{
    // Where the images that are generated are sent.
    public List<DiffusionTextureChanger> targets;

    // true if we add the images that generated to the previous image rotation of a target.
    public bool addToTextureTotal = false;

    // Number of image variations for one generation(same prompt, model, etc.)
    public int numOfVariations = 1;

    // Positive and negative text prompts
    public string positivePrompt;
    public string negativePrompt;

    // TODO upload an Image list instead of just 2? outpainting?
    // We upload up-to two images to the server
    public List<Texture2D> uploadTextures;
    //public Texture2D uploadImage;
    //public Texture2D secondUploadImage;

    // Denoising parameter for the input image
    public float denoise = 1.0f;

    // TODO decide, do I need diffusionModels separate from the workflows? what about models that don't work with certain workflows? do we disregard?
    // TODO just put everything on the diffusionWorkFlows?
    // Checkpoint model to be used in the generation
    public diffusionModels diffusionModel;

    // Workflow type to be used in the generation.
    public diffusionWorkflows diffusionJsonType;

    // Used for sending any type of information that might be unique to a Request
    public string SpecialInput;

    [System.NonSerialized]
    public List<Texture2D> textures;
    [System.NonSerialized]
    public bool finishedRequest = false;
    [System.NonSerialized]
    public int requestNum = -1;
    [System.NonSerialized]
    public string diffImgName;
    [System.NonSerialized]
    public string prompt_id;
    [System.NonSerialized]
    public Collision collision = null;
    [System.NonSerialized]
    public DiffusableObject diffusableObject = null;

    public DiffusionRequest()
    {
        targets = new List<DiffusionTextureChanger>();
        textures = new List<Texture2D>();
        uploadTextures = new List<Texture2D>();
    }

    public DiffusionRequest(List<DiffusionTextureChanger> curTargets)
    {
        targets = curTargets;
        textures = new List<Texture2D>();
        uploadTextures = new List<Texture2D>();
    }
}

public class ComfyOrganizer : MonoBehaviour
{
    public Dictionary<int, DiffusionRequest> DiffuseDictionary;
    public ComfySceneLibrary comfyLib;

    private static int currentRequestNum = 0;
    private List<string> allTextureNames;
    private static int currentTextureNameNumber = 0;

    private void Awake()
    {
        DiffuseDictionary = new Dictionary<int, DiffusionRequest>();
        allTextureNames = new List<string>();
    }

    // TODO description
    private string GetDiffusionImageName(DiffusionRequest diffReq)
    {
        string retName = "Generated_" + diffReq.requestNum;
        return retName;
    }

    /// <summary>
    /// Returns a unique name for an Image and adds it to the total list of texture names
    /// </summary>
    public string UniqueImageName()
    {
        string newTextureName = "DiffImage_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + '_' + currentTextureNameNumber.ToString();
        allTextureNames.Add(newTextureName);

        currentTextureNameNumber++;

        return newTextureName;
    }

    /// <summary>
    /// Returns a deep copy of the given Diffusion Request
    /// </summary>
    /// <param name="diffusionRequest">Diffusion Request to deep copy</param>
    public DiffusionRequest copyDiffusionRequest(DiffusionRequest diffusionRequest)
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.targets = diffusionRequest.targets;
        newDiffusionRequest.addToTextureTotal = diffusionRequest.addToTextureTotal;
        newDiffusionRequest.numOfVariations = diffusionRequest.numOfVariations;
        newDiffusionRequest.positivePrompt = diffusionRequest.positivePrompt;
        newDiffusionRequest.negativePrompt = diffusionRequest.negativePrompt;

        newDiffusionRequest.denoise = diffusionRequest.denoise;
        newDiffusionRequest.requestNum = diffusionRequest.requestNum;

        newDiffusionRequest.diffusionModel = diffusionRequest.diffusionModel;
        newDiffusionRequest.diffusionJsonType = diffusionRequest.diffusionJsonType;

        newDiffusionRequest.collision = diffusionRequest.collision;
        newDiffusionRequest.diffusableObject = diffusionRequest.diffusableObject;

        // Texture2D deep copying --------------------------------------------------------------------
        newDiffusionRequest.textures = new List<Texture2D>();
        foreach (Texture2D texture in diffusionRequest.textures)
        {
            Texture2D copyTexture = new Texture2D(texture.width, texture.height);
            copyTexture.SetPixels(texture.GetPixels());
            copyTexture.Apply();
            copyTexture.name = texture.name;

            newDiffusionRequest.textures.Add(copyTexture);
        }

        newDiffusionRequest.uploadTextures = new List<Texture2D>();
        foreach (Texture2D texture in diffusionRequest.uploadTextures)
        {
            Texture2D copyTexture = new Texture2D(texture.width, texture.height);
            copyTexture.SetPixels(texture.GetPixels());
            copyTexture.Apply();
            copyTexture.name = texture.name;

            newDiffusionRequest.uploadTextures.Add(copyTexture);
        }
        // Texture2D deep copying --------------------------------------------------------------------


        newDiffusionRequest.finishedRequest = diffusionRequest.finishedRequest;
        newDiffusionRequest.diffImgName = diffusionRequest.diffImgName;
        newDiffusionRequest.prompt_id = diffusionRequest.prompt_id;

        newDiffusionRequest.SpecialInput = diffusionRequest.SpecialInput;

        return newDiffusionRequest;
    }

    /// <summary>
    /// Used for loading models into RAM to speed up subsequent image generations using the same model. 
    /// Sends a minimal image generation request with a specified model to load it into RAM.
    /// </summary>
    /// <param name="curModel">Model to load onto RAM</param>
    public void SendMinimalDiffusionRequest(diffusionModels curModel)
    {
        DiffusionRequest diffusionRequest = new DiffusionRequest();
        diffusionRequest.diffusionModel = curModel;
        diffusionRequest.numOfVariations = 1;
        SendDiffusionRequest(diffusionRequest);
    }

    /// <summary>
    /// Sends an image generation request to the ComfySceneLibrary. The Images that are created are then added to the targets list of the diffusionRequest.
    /// </summary>
    /// <param name="diffusionRequest">Diffusion Request that is sent to the ComfySceneLibrary</param>
    public void SendDiffusionRequest(DiffusionRequest diffusionRequest)
    {
        //Debug.Log("wiwi " + diffusionRequest.diffusionJsonType.ToString());
        // TODO choose who has the responsibility for defining the various parameters of a diffusion request, the GameObject? whoever?
        DiffusionRequest newDiffusionRequest = copyDiffusionRequest(diffusionRequest);
        //Debug.Log("wiwi2 " + newDiffusionRequest.diffusionJsonType.ToString());
        newDiffusionRequest.requestNum = currentRequestNum;
        newDiffusionRequest.diffImgName = GetDiffusionImageName(newDiffusionRequest);
        DiffuseDictionary.Add(currentRequestNum, newDiffusionRequest);
        currentRequestNum++;


        int PROMPT_TRIAL_NUM = 5;
        StartCoroutine(comfyLib.QueuePromptCoroutine(newDiffusionRequest, PROMPT_TRIAL_NUM));
    }

    /// <summary>
    /// Gets a list of the Diffusion Requests that have still not been generated.
    /// </summary>
    public List<DiffusionRequest> GetUnfinishedRequestPrompts()
    {
        // TODO take into account ONLY the requests that were sent to generation?
        List<DiffusionRequest> relevantKeys = new List<DiffusionRequest>();
        List<DiffusionRequest> finishedKeys = new List<DiffusionRequest>();

        foreach (var diffReqID in DiffuseDictionary)
        {
            if (!diffReqID.Value.finishedRequest)
            {
                relevantKeys.Add(diffReqID.Value);
            }
            else
            {
                finishedKeys.Add(diffReqID.Value);
            }
        }
        foreach (var key in finishedKeys)
        {
            DiffuseDictionary.Remove(key.requestNum);
        }

        return relevantKeys;
    }

    /// <summary>
    /// Adds a texture that was generated for a Diffusion Request to that Diffusion Request.
    /// </summary>
    /// <param name="texture">Texture that was created according to and added to the Diffusion Request</param>
    /// <param name="diffusionRequest">Current Diffusion Request forwhich the texture was created</param>
    public void AddImage(Texture2D texture, DiffusionRequest diffusionRequest)
    {
        if (texture == null || diffusionRequest == null)
        {
            return;
        }

        int requestNum = diffusionRequest.requestNum;
        if (!DiffuseDictionary.ContainsKey(requestNum))
        {
            return;
        }
        if (DiffuseDictionary[requestNum].finishedRequest)
        {
            return;
        }

        if (DiffuseDictionary[requestNum].numOfVariations > DiffuseDictionary[requestNum].textures.Count)
        {
            DiffuseDictionary[requestNum].textures.Add(texture);
        }
        if (DiffuseDictionary[requestNum].numOfVariations <= DiffuseDictionary[requestNum].textures.Count)
        {
            DiffuseDictionary[requestNum].finishedRequest = true;
            SendTexturesToRecipient(DiffuseDictionary[requestNum]);
        }
    }

    /// <summary>
    /// Sends the textures of a Diffusion Request to its targets.
    /// </summary>
    /// <param name="diffusionRequest">Diffusion Request to send its textures to its targets</param>
    private void SendTexturesToRecipient(DiffusionRequest diffusionRequest)
    {
        if (!diffusionRequest.finishedRequest || diffusionRequest.targets == null)
        {
            Debug.LogError("Add target to send textures to");
            return;
        }

        if (diffusionRequest.targets.Count == 0)
        {
            return;
        }
        foreach(DiffusionTextureChanger changer in diffusionRequest.targets)
        {
            changer.AddTexture(diffusionRequest);
        }

        diffusionRequest.targets.Clear();
    }
}
