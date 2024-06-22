using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Rendering;
using UnityEngine;

public class AIGadgetAssistant : MonoBehaviour
{
    public string AIAudioClipFolder = "Assets/Sounds/Voiceover";

    private DiffusionTextureChanger diffusionTextureChanger;
    private Dictionary<string, AudioClip> AIAudioClips;

    private static string DEFAULT_POSITIVE_PROMPT = "masterpiece,high quality,highres,solo,pslain,x hair ornament,brown eyes,dress,hoop,black dress,strings,floating circles,blue orbs,turning around,detached sleeves,black background, short hair,luminous hair,blonde hair,smile";
    private static string DEFAULT_NEGATIVE_PROMPT = "EasyNegativeV2,negative_hand-neg,(low quality, worst quality:1.2)";

    private void Awake()
    {
        if (AIAudioClipFolder == null || AIAudioClipFolder == "")
        {
            Debug.LogError("Choose a AI Audio Clip Folder");
            return;
        }
        diffusionTextureChanger = gameObject.AddComponent<DiffusionTextureChanger>();
        AIAudioClips = new Dictionary<string, AudioClip>();
        GetAudioClips(AIAudioClipFolder);
    }

    private void Start()
    {
        InvokeRepeating("REMOVETHISFUNC", 2, 2);
    }

    private void GetAudioClips(string audioClipFolder)
    {
        var audioClipFileNames = Directory.GetFiles(audioClipFolder, "*.wav"); // TODO notice that this is WAV, but might be other formats??

        foreach(string audioClipName in audioClipFileNames)
        {
            AIAudioClips[audioClipName] = Resources.Load(audioClipFolder + "/" + audioClipName) as AudioClip;
        }        
    }

    public void REMOVETHISFUNC()
    {
        CreateAITexture();
        AITalk();
    }

    public void CreateAITexture(string keywords = "")
    {
        DiffusionRequest diffusionRequest = new DiffusionRequest();
        diffusionRequest.diffusionModel = diffusionModels.ghostmix;

        // TODO need to ADD keywords to an existing prompt?
        diffusionRequest.positivePrompt = DEFAULT_POSITIVE_PROMPT + keywords;
        diffusionRequest.negativePrompt = DEFAULT_NEGATIVE_PROMPT;

        diffusionRequest.targets.Add(diffusionTextureChanger);
        diffusionRequest.addToTextureTotal = true;
        diffusionRequest.diffusionJsonType = diffusionWorkflows.AIAssistant;

        GeneralGameScript.instance.comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }

    public void AITalk(string audioClipName = "")
    {
        List<Texture2D> curTextures = diffusionTextureChanger.GetTextures();
        if (curTextures.Count == 0)
        {
            return;
        }

        Texture2D currentTexture = curTextures[curTextures.Count - 1];        

        GeneralGameScript.instance.uiDiffusionTexture.CreatePopup(new List<Texture2D>() { currentTexture });
    }
}
