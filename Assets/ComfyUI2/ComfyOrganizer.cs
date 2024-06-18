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
[Serializable]
public class DiffusionRequest
{
    public List<DiffusionTextureChanger> targets = new List<DiffusionTextureChanger>();
    public bool addToTextureTotal = false;
    public int numOfVariations = 1;

    public string positivePrompt;
    public string negativePrompt;

    /*public string uploadImageName;
    public string secondUploadImageName;*/

    public Texture2D uploadImage;
    public Texture2D secondUploadImage;

    public float denoise = 1.0f;

    // TODO decide, do I need diffusionModels separate from the workflows? what about models that don't work with certain workflows? do we disregard?
    // TODO just put everything on the diffusionWorkFlows?
    public diffusionModels diffusionModel;

    public diffusionWorkflows diffusionJsonType;

    [System.NonSerialized]
    public List<Texture2D> textures = new List<Texture2D>();
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
}

public class ComfyOrganizer : MonoBehaviour
{
    public Dictionary<int, DiffusionRequest> DiffuseDictionary = new Dictionary<int, DiffusionRequest>();
    public ComfySceneLibrary comfyLib;

    private static int currentRequestNum = 0;

    private List<string> allTextureNames = new List<string>();
    private static int currentTextureNameNumber = 0;

    private string GetDiffusionImageName(DiffusionRequest diffReq)
    {
        string retName = "Generated_" + diffReq.requestNum;
        return retName;
    }

    public string UniqueImageName()
    {
        string newTextureName = "DiffImage_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + '_' + currentTextureNameNumber.ToString();
        allTextureNames.Add(newTextureName);

        currentTextureNameNumber++;

        return newTextureName;
    }

    public void addDiffusionRequestToDatabase(DiffusionRequest diffReq, int currentRequestNum)
    {
        DiffuseDictionary.Add(currentRequestNum, diffReq);
    }

    public DiffusionRequest copyDiffusionRequest(DiffusionRequest diffReq)
    {
        DiffusionRequest newDiffReq = new DiffusionRequest();

        newDiffReq.targets = diffReq.targets;
        newDiffReq.addToTextureTotal = diffReq.addToTextureTotal;
        newDiffReq.numOfVariations = diffReq.numOfVariations;
        newDiffReq.positivePrompt = diffReq.positivePrompt;
        newDiffReq.negativePrompt = diffReq.negativePrompt;

        //newDiffReq.uploadImageName = diffReq.uploadImageName;

        newDiffReq.denoise = diffReq.denoise;
        newDiffReq.requestNum = diffReq.requestNum;

        newDiffReq.diffusionModel = diffReq.diffusionModel;
        newDiffReq.diffusionJsonType = diffReq.diffusionJsonType;

        // Texture2D deep copying --------------------------------------------------------------------
        newDiffReq.textures = new List<Texture2D>();
        foreach (Texture2D texture in diffReq.textures)
        {
            Texture2D copyTexture = new Texture2D(texture.width, texture.height);
            copyTexture.SetPixels(texture.GetPixels());
            copyTexture.Apply();
            copyTexture.name = texture.name;

            newDiffReq.textures.Add(copyTexture);
        }
        if (diffReq.uploadImage != null)
        {
            Texture2D uploadCopyTexture = new Texture2D(diffReq.uploadImage.width, diffReq.uploadImage.height);
            uploadCopyTexture.SetPixels(diffReq.uploadImage.GetPixels());
            uploadCopyTexture.Apply();
            uploadCopyTexture.name = diffReq.uploadImage.name;
            newDiffReq.uploadImage = uploadCopyTexture;
        }
        if (diffReq.secondUploadImage != null)
        {
            Texture2D uploadSecondCopyTexture = new Texture2D(diffReq.secondUploadImage.width, diffReq.secondUploadImage.height);
            uploadSecondCopyTexture.SetPixels(diffReq.secondUploadImage.GetPixels());
            uploadSecondCopyTexture.Apply();
            uploadSecondCopyTexture.name = diffReq.secondUploadImage.name;
            newDiffReq.secondUploadImage = uploadSecondCopyTexture;

        }
        // Texture2D deep copying --------------------------------------------------------------------


        newDiffReq.finishedRequest = diffReq.finishedRequest;
        newDiffReq.diffImgName = diffReq.diffImgName;
        newDiffReq.prompt_id = diffReq.prompt_id;

        return newDiffReq;
    }

    // TODO choose who has the responsibility for defining the various parameters of a diffusion request, the GameObject? whoever?
    public void SendDiffusionRequest(DiffusionRequest diffReq)
    {
        DiffusionRequest newDiffReq = copyDiffusionRequest(diffReq);

        newDiffReq.requestNum = currentRequestNum;
        newDiffReq.diffImgName = GetDiffusionImageName(newDiffReq);
        addDiffusionRequestToDatabase(newDiffReq, currentRequestNum);
        currentRequestNum++;

        StartCoroutine(comfyLib.QueuePromptCoroutine(newDiffReq));
    }

    public List<DiffusionRequest> GetUnfinishedRequestPrompts()
    {
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

    public void AddImage(Texture2D texture, DiffusionRequest diffReq)
    {
        if (texture == null || diffReq == null)
        {
            return;
        }

        int requestNum = diffReq.requestNum;
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

    private void SendTexturesToRecipient(DiffusionRequest diffReq)
    {
        if (!diffReq.finishedRequest || diffReq.targets == null)
        {
            Debug.LogError("Add target to send textures to");
            return;
        }

        foreach(DiffusionTextureChanger changer in diffReq.targets)
        {
            changer.AddTexture(diffReq);
        }
    }
}
