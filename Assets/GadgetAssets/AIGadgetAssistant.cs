using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Rendering;
using UnityEngine;
using static GeneralGameLibraries;

public class AIGadgetAssistant : MonoBehaviour
{
    public string AIAudioClipFolder = "Sounds/Voiceover";
    private GeneralGameLibraries.AudioClipsLibrary AudioClipsLibrary;

    private DiffusionTextureChanger diffusionTextureChanger;

    private static string DEFAULT_POSITIVE_PROMPT = "masterpiece,high quality,highres,solo,pslain,x hair ornament,brown eyes,dress,hoop,black dress,strings,floating circles,blue orbs,turning around,detached sleeves,black background, short hair,luminous hair,blonde hair,smile";
    private static string DEFAULT_NEGATIVE_PROMPT = "EasyNegativeV2,negative_hand-neg,(low quality, worst quality:1.2)";

    private void Awake()
    {
        AudioClipsLibrary = new GeneralGameLibraries.AudioClipsLibrary(AIAudioClipFolder);
        diffusionTextureChanger = gameObject.AddComponent<DiffusionTextureChanger>();
    }

    /// <summary>
    /// Sends an Image generation request for the AI representation, according to the input keywords
    /// </summary>
    /// <param name="keywords">To be added to the prompts of the Image Generation request</param>
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

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }

    /// <summary>
    /// Plays an AI assistant Audio Clip and creates a popup to go along with it
    /// </summary>
    /// <param name="audioClipName">AI Assistant Audio Clip to be played</param>
    public void AITalk(string audioClipName = "")
    {
        List<Texture2D> curTextures = diffusionTextureChanger.GetTextures();
        if (curTextures.Count == 0)
        {
            return;
        }

        Texture2D currentTexture = curTextures[curTextures.Count - 1];

        GameManager.getInstance().uiDiffusionTexture.CreatePopup(new List<Texture2D>() { currentTexture });

        if (audioClipName == "")
        {
            return;
        }
        GameManager.getInstance().headAudioSource.PlayOneShot(AudioClipsLibrary.AudioClips[audioClipName]);
    }
}
