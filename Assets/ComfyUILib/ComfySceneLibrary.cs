using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}

public enum diffusionWorkflows
{
    txt2img,
    txt2imgLCM,
    img2img,
    img2imgLCM,
    combineImages,

    // Assuming these workflows will not use low powered models and thus no need for LCM
    baseCamera,
    depthCamera,
    openpose,
    outpainting,

    // Gadget AI representation
    AIAssistant
}

public enum diffusionModels
{
    nano,
    mini,
    turbo,
    turblxl,
    ghostmix
}

public class ComfySceneLibrary : MonoBehaviour
{
    public string serverAddress = "127.0.0.1:8188";  //"jonathanmiroshnik-backpropagation-09103750.thinkdiffusion.xyz"
    public ComfyOrganizer comfyOrg;

    public string JSONFolderPath = "Assets/ComfyUILib/JSONMain";
    public string ImageFolderName = "Assets/";

    private string clientId;
    private ClientWebSocket ws;
    private bool started_generations = false;
    private Dictionary<diffusionWorkflows, string> diffusionJsons;

    private bool uploadingImage = false;

    private bool readyForDiffusion = false;

    private void Awake()
    {
        clientId = Guid.NewGuid().ToString();
        ws = new ClientWebSocket();
        diffusionJsons = new Dictionary<diffusionWorkflows, string>();
    }

    // TODO notice that this START must always come BEFORE(put the library before the organizer in the node properties)
    // TODO cont. the ComfyOrganizer or else some things will not be ready for an instant diffusion request
    private void Start()
    {
        serverAddress = GameManager.getInstance().IP;

        if (serverAddress == "")
        {
            Debug.LogError("Error! given IP in game manager is empty!");
        }
        else
        {
            Debug.Log("Passed the IP: " + serverAddress);
        }
        
        // Get all enum adjacent JSON workflows
        var jsonFiles = Directory.GetFiles(JSONFolderPath, "*.json");

        foreach (var file in jsonFiles)
        {
            string fileName = Path.GetFileName(file);
            string fileContent = File.ReadAllText(file);

            int dotIndex = fileName.LastIndexOf('.');
            string splitName = fileName.Substring(0, dotIndex);
            if (Enum.IsDefined(typeof(diffusionWorkflows), splitName))
            {
                diffusionWorkflows enumVal;
                Enum.TryParse<diffusionWorkflows>(splitName, out enumVal);
                diffusionJsons.Add(enumVal, fileContent);
            }
            else
            {
                // TODO check why this error is not reached and instead getting a different dictionary type error
                Debug.LogError("Please add JSON workflow " + splitName.ToString() + " to the diffusionJsons enum");
            }
        }

        readyForDiffusion = true;

        StartServerConnection();
    }

    /// <summary>
    /// Starts a connection to the server
    /// </summary>
    private async void StartServerConnection()
    {
        await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);
        StartListening();
    }

    /// <summary>
    /// Gets the JSON workflow corresponding to the Diffusion Workflow
    /// </summary>
    /// <param name="enumValName">Diffusion Workflow to get JSON of</param>
    private string getWorkflowJSON(diffusionWorkflows enumValName)
    {
        string ret_str = "";
        if (Enum.IsDefined(typeof(diffusionWorkflows), enumValName))
        {
            ret_str = diffusionJsons[enumValName];
        }

        return ret_str;
    }

    private string DiffusionJSONFactory(DiffusionRequest diffReq)
    {
        string guid = Guid.NewGuid().ToString();
        string promptText = $@"
        {{
            ""id"": ""{guid}"",
            ""prompt"": {getWorkflowJSON(diffReq.diffusionJsonType)}
        }}";
        JObject json = JObject.Parse(promptText);

        // TODO notice that curImageSize will need to change in a situation like outpainting

        string curDiffModel = "";
        Vector2Int curImageSize = Vector2Int.zero;
        switch (diffReq.diffusionModel)
        {
            case diffusionModels.nano:
                curDiffModel = "stable-diffusion-nano-2-1.ckpt";
                curImageSize = new Vector2Int(128, 128);
                break;
            case diffusionModels.mini:
                curDiffModel = "miniSD.ckpt";
                curImageSize = new Vector2Int(256, 256);
                break;
            case diffusionModels.turbo:
                curDiffModel = "sdTurbo_v10.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;
            case diffusionModels.turblxl:
                // TODO add this model
                curDiffModel = "??";
                curImageSize = new Vector2Int(1024, 1024);
                break;
            case diffusionModels.ghostmix:
                curDiffModel = "ghostmix_v20Bakedvae.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;
        }

        if (curDiffModel == null || curDiffModel == "" || curImageSize == Vector2Int.zero)
        {
            Debug.LogError("You must choose a useable Diffusion model");
            return null;
        }

        string randomSeed = UnityEngine.Random.Range(1, 10000).ToString();
        //TODO add all cases according to diffusionWorkflows ENUM
        switch (diffReq.diffusionJsonType)
        {
            case diffusionWorkflows.txt2imgLCM:
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;

                json["prompt"]["5"]["inputs"]["width"] = curImageSize.x;
                json["prompt"]["5"]["inputs"]["height"] = curImageSize.y;
                break;

            case diffusionWorkflows.img2imgLCM:
                /*if (diffReq.uploadImageName == null || diffReq.uploadImageName == "")
                {
                    Debug.LogError("Make sure a valid uploadImage is part of the Diffusion Request before upload it");
                    return null;
                }*/

                if (diffReq.uploadImage == null)
                {
                    Debug.LogError("Make sure a valid uploadImage is part of the Diffusion Request before upload it");
                    return null;
                }

                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["3"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;
                json["prompt"]["15"]["inputs"]["amount"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;
                
                StartCoroutine(UploadImage(diffReq.uploadImage));
                json["prompt"]["11"]["inputs"]["image"] = diffReq.uploadImage.name;
                break;

            case diffusionWorkflows.txt2img:
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;

                json["prompt"]["5"]["inputs"]["width"] = curImageSize.x;
                json["prompt"]["5"]["inputs"]["height"] = curImageSize.y;
                break;

            case diffusionWorkflows.combineImages:
                if (diffReq.uploadImage == null || (diffReq.secondUploadImage == null))
                {
                    Debug.LogError("Make sure a valid uploadImage or secondUploadImage is part of the Diffusion Request before upload it");
                    return null;
                }

                json["prompt"]["1"]["inputs"]["ckpt_name"] = curDiffModel;
                json["prompt"]["2"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["3"]["inputs"]["text"] = diffReq.negativePrompt;

                json["prompt"]["50"]["inputs"]["amount"] = diffReq.numOfVariations;
                json["prompt"]["51"]["inputs"]["amount"] = diffReq.numOfVariations;

                StartCoroutine(UploadImage(diffReq.uploadImage));
                StartCoroutine(UploadImage(diffReq.secondUploadImage));
                // Input Image:
                json["prompt"]["12"]["inputs"]["image"] = diffReq.uploadImage.name;
                // Style is extracted from this Image:
                json["prompt"]["41"]["inputs"]["image"] = diffReq.secondUploadImage.name;

                json["prompt"]["21"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["21"]["inputs"]["seed"] = randomSeed;
                break;

            case diffusionWorkflows.AIAssistant:
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;
                
                curImageSize.x = 512;
                curImageSize.y = 768;
                json["prompt"]["5"]["inputs"]["width"] = curImageSize.x;
                json["prompt"]["5"]["inputs"]["height"] = curImageSize.y;
                break;

            default:
                Debug.LogError("Please choose a useable Diffusion workflow");
                return null;
        }

        return json.ToString();
    }

    public IEnumerator QueuePromptCoroutine(DiffusionRequest diffReq)
    {
        if (!readyForDiffusion)
        {
            yield return null;
        }

        string url = "http://" + serverAddress + "/prompt";

        string promptText = DiffusionJSONFactory(diffReq);
        while (uploadingImage)
        {
            yield return null;
            //yield return new WaitForSeconds(0.02f);
        }

        if (promptText == null || promptText.Length <= 0)
        {
            yield return null;
        }
        else
        {
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                //Debug.Log("Prompt queued successfully." + request.downloadHandler.text);

                // This is the only use of ResponseData, but it is needed for proper downloading of the prompt
                ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
                diffReq.prompt_id = data.prompt_id;
            }

            yield break;
        }
    }


    // Used to save up on compute when not using the image generation
    private IEnumerator SmallWait()
    {
        yield return new WaitForSeconds(0.1f);
    }

    private async void StartListening()
    {
        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result = null;

        while (ws.State == WebSocketState.Open)
        {
            var stringBuilder = new StringBuilder();
            do
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                }
                else
                {
                    var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    stringBuilder.Append(str);
                }
            }
            while (!result.EndOfMessage);

            string response = stringBuilder.ToString();

            // When images are not being generated, this statement makes the program wait a bit which saves on compute
            if (response.Contains("\"queue_remaining\": 0"))
            {
                StartCoroutine(SmallWait());
            }

            // Goes over each prompt that needs completing and checks whether it has completed,
            // if it did, it downloads the images and labels the promptID as true(as in, finished).
            List<DiffusionRequest> allDiffReqs = comfyOrg.GetUnfinishedRequestPrompts();
            foreach (DiffusionRequest diffReq in allDiffReqs)
            {
                RequestFileName(diffReq);
            }
        }
    }

    void OnDestroy()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }

    public void RequestFileName(DiffusionRequest diffReq)
    {
        StartCoroutine(RequestFileNameRoutine(diffReq));
    }

    IEnumerator RequestFileNameRoutine(DiffusionRequest diffReq)
    {
        string url = "http://" + serverAddress + "/history/" + diffReq.prompt_id;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    if (started_generations)
                    {
                        Debug.LogError(": HTTP Error: " + webRequest.error);
                    }
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);

                    // Jonathan - added the for loop to coincide with the changes to the ExtractFilename function becoming a batch-size dependant downloader
                    // Jonathan - another change, download all the images FIRST, then display, to test display speed
                    string[] filenames = ExtractFilename(webRequest.downloadHandler.text);

                    //Debug.Log("All File Names:");
                    //foreach (string item in filenames) { 
                    //Debug.Log(item);
                    //}

                    // If there are no filenames, the prompt has not yet finished generating
                    if (filenames.Length <= 0)
                    {
                        break;
                    }

                    // Downloading each image of the prompt
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        string imageURL = "http://" + serverAddress + "/view?filename=" + filenames[i];
                        StartCoroutine(DownloadImage(imageURL, diffReq));
                    }
                    break;
            }
        }
    }

    string[] ExtractFilename(string jsonString)
    {
        // Jonathan - Changed this from returning a single filename to all the filenames in the output - with the for loop
        string keyToLookFor = "\"filename\":";
        int total_files = Regex.Matches(jsonString, keyToLookFor).Count;

        string[] filenames = new string[total_files];
        int prevIndex = -1;

        for (int i = 0; i < total_files; i++)
        {
            // Step 1: Identify the part of the string that contains the "filename" key
            int startIndex = jsonString.IndexOf(keyToLookFor, prevIndex + 1);
            prevIndex = startIndex;

            if (startIndex == -1)
            {
                return null;
            }

            // Adjusting startIndex to get the position right after the keyToLookFor
            startIndex += keyToLookFor.Length;

            // Step 2: Extract the substring starting from the "filename" key
            string fromFileName = jsonString.Substring(startIndex);

            // Assuming that filename value is followed by a comma (,)
            int endIndex = fromFileName.IndexOf(',');

            // Extracting the filename value (assuming it's wrapped in quotes)
            string filenameWithQuotes = fromFileName.Substring(0, endIndex).Trim();

            // Removing leading and trailing quotes from the extracted value
            filenames[i] = filenameWithQuotes.Trim('"');

        }

        return filenames;
    }

    IEnumerator DownloadImage(string imageUrl, DiffusionRequest diffReq)
    {
        while (uploadingImage)
        {
            yield return null;
            //yield return new WaitForSeconds(0.02f);
        }

        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Get the downloaded texture
                Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                // Adding the texture to the texture queue
                comfyOrg.AddImage(texture, diffReq);
            }
            else
            {
                Debug.LogError("Image download failed: " + webRequest.error);
            }
        }
    }

    

    private IEnumerator UploadImage(Texture2D curTexture)
    {        
        string url = "http://" + serverAddress + "/upload/image";

        WWWForm form = new WWWForm();

        form.AddBinaryData("image", curTexture.EncodeToPNG(), curTexture.name, "image/png");
        form.AddField("type", "input");
        form.AddField("overwrite", "false");
        uploadingImage = true;

        using (var unityWebRequest = UnityWebRequest.Post(url, form))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(unityWebRequest.error);
            }
            else
            {
                uploadingImage = false;
                //Debug.Log("Image Upload succesful");
            }
        }

        uploadingImage = false;
    }

    /*private IEnumerator UploadImage(string imgName)
    {
        string url = "http://" + serverAddress + "/upload/image";

        WWWForm form = new WWWForm();

        form.AddBinaryData("image", System.IO.File.ReadAllBytes(ImageFolderName + '/' + imgName), imgName, "image/png");
        form.AddField("type", "input");
        form.AddField("overwrite", "false");

        uploadingImage = true;

        using (var unityWebRequest = UnityWebRequest.Post(url, form))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(unityWebRequest.error);
            }
            else
            {
                uploadingImage = false;
                //Debug.Log("Image Upload succesful");
            }
        }

        uploadingImage = false;
    }*/
}
