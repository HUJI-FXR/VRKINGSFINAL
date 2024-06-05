using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;
using System.Net;


// TODO maybe remove requestNum from this class?
[Serializable]
public class DiffusionRequest
{
    public DiffusionTextureChanger target;
    public bool addToTextureTotal = false;
    public int numOfVariations = 1;

    public string positivePrompt;
    public string negativePrompt;

    public string uploadImageName;

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
}

public class ComfyOrganizer : MonoBehaviour
{
    public Dictionary<int, DiffusionRequest> DiffuseDictionary = new Dictionary<int, DiffusionRequest>();
    public ComfySceneLibrary comfyLib;

    private static int currentRequestNum = 0;

    private string GetDiffusionImageName(DiffusionRequest diffReq)
    {
        string retName = "Generated_" + diffReq.requestNum;
        return retName;
    }

    public void addDiffusionRequestToDatabase(DiffusionRequest diffReq, int currentRequestNum)
    {
        DiffuseDictionary.Add(currentRequestNum, diffReq);
    }

    // TODO choose who has the responsibility for defining the various parameters of a diffusion request, the GameObject? whoever?
    public void SendDiffusionRequest(DiffusionRequest diffReq)
    {
        diffReq.requestNum = currentRequestNum;
        diffReq.diffImgName = GetDiffusionImageName(diffReq);
        addDiffusionRequestToDatabase(diffReq, currentRequestNum);
        currentRequestNum++;

        StartCoroutine(comfyLib.QueuePromptCoroutine(diffReq));
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
        if (!diffReq.finishedRequest || diffReq.target == null)
        {
            return;
        }

        // TODO leave the responsibility to target
        /*DiffusionTextureChanger curTextureChanger = diffReq.gameObject.GetComponent<DiffusionTextureChanger>();
        if (curTextureChanger == null)
        {
            // TODO error message?
            return;
        }*/

        diffReq.target.AddTexture(diffReq.textures, diffReq.addToTextureTotal);
    }

    // -------------------------------------------------------------------------------- TODO CHANGE UNDER


    // TODO Change of texture functionality is DIFFERENT and should happen in a DIFFERENT place than creating and downloading he imagery
    /*void DelayedChangeToTexture()
    {
        for (int i = 0; i < TextureLists.Length; i++)
        {
            Transform parentObjectTransform = TextureLists[i].ParentObject.transform;
            int numberOfChildren = parentObjectTransform.childCount;

            // If the ParentObject does not have children, change its own texture
            GameObject block_to_transform = TextureLists[i].ParentObject;

            if (numberOfChildren > 0)
            {
                // Get a random child out of the children of the ParentObject to change its texture
                block_to_transform = parentObjectTransform.GetChild(UnityEngine.Random.Range(0, numberOfChildren)).gameObject;
            }

            if (block_to_transform != null & TextureLists[i].textures != null)
            {
                if (TextureLists[i].textures.Count <= 0)
                {
                    continue;
                }

                Renderer cur_block_renderer = block_to_transform.GetComponent<Renderer>();
                Texture2D cur_texture = TextureLists[i].textures[UnityEngine.Random.Range(0, TextureLists[i].textures.Count)];

                // TODO change in regards to various Shader types that might come, also change it so that doesn't require every block to have a script(only one script for all blocks)
                if (cur_block_renderer.material.shader.name == "Custom/Fade")
                {
                    // TODO - Checks if there is the CrossFade script, bad code, needs to change look at one TODO above
                    CrossFade cur_crossfade = block_to_transform.GetComponent<CrossFade>();
                    if (cur_crossfade != null)
                    {
                        if (cur_crossfade.textures.Count < 3)
                        {
                            cur_crossfade.textures.Add(cur_texture);
                        }
                    }
                }
                else
                {
                    // TODO notice the many Magic numbers and names that need to become CONSTANT etc
                    cur_block_renderer.material.SetTexture("_MainTex", cur_texture);
                }
            }
        }
    }*/
}
