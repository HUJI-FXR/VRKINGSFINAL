using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}

public class FileExistsChecker
{
    public bool fileExists = false;
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

    // A special two-type mechanism
    outpainting,
    grid4Outpainting,

    // Gadget AI representation
    AIAssistant
}

public enum diffusionModels
{
    nano,
    mini,
    turbo,
    turblxl,
    ghostmix,
    thinkdiffusiontest
}


// TODO remove LogErrors with something that doesn't force out the user in the game.

public class ComfySceneLibrary : MonoBehaviour
{
    private string HTTPPrefix = "https://";  // https://  ------ When using online API service
    public ComfyOrganizer comfyOrg;

    public string serverAddress = "";

    private string JSONFolderPath = "JSONMain";
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
    public void StartComfySceneLibrary()
    {
        // TODO delete this PREFIX before full release
        string THINKDIFFUSION_PREFIX = "jonathanmiroshnik-";
        string THINKDIFFUSION_POSTFIX = ".thinkdiffusion.xyz";

        if (serverAddress != "")
        {
            GameManager.getInstance().IP = serverAddress;
        }

        if (GameManager.getInstance().IP == "" || GameManager.getInstance().IP == "127.0.0.1:8188")
        {
            GameManager.getInstance().IP = "127.0.0.1:8188";
            HTTPPrefix = "http://";
        }
        else
        {
            GameManager.getInstance().IP = THINKDIFFUSION_PREFIX + GameManager.getInstance().IP + THINKDIFFUSION_POSTFIX;
            HTTPPrefix = "https://";
        }

        // Get all enum adjacent JSON workflows
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(JSONFolderPath);

        foreach (var file in jsonFiles)
        {
            string fileName = file.name;
            string fileContent = file.text;

            if (Enum.IsDefined(typeof(diffusionWorkflows), fileName))
            {
                diffusionWorkflows enumVal;
                Enum.TryParse<diffusionWorkflows>(fileName, out enumVal);
                diffusionJsons.Add(enumVal, fileContent);
            }
            else
            {
                // TODO check why this error is not reached and instead getting a different dictionary type error
                Debug.LogError("Please add JSON workflow " + fileName.ToString() + " to the diffusionJsons enum");
            }
        }

        readyForDiffusion = true;

        StartCoroutine(DownloadCycle());

        //StartServerConnection();
    }


    public IEnumerator DownloadCycle()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);

            while (uploadingImage)
            {
                //yield return null;
                yield return new WaitForSeconds(0.02f);
            }

            List<DiffusionRequest> allDiffReqs = comfyOrg.GetUnfinishedRequestPrompts();
            foreach (DiffusionRequest diffReq in allDiffReqs)
            {
                RequestFileName(diffReq);
            }
        }
    }

    /// <summary>
    /// Gets the JSON workflow corresponding to the Diffusion Workflow
    /// </summary>
    /// <param name="enumValName">Diffusion Workflow to get JSON of</param>
    private string getWorkflowJSON(diffusionWorkflows enumValName)
    {
        string ret_str = "";
        if (diffusionJsons.ContainsKey(enumValName))
        {
            ret_str = diffusionJsons[enumValName];
        }

        return ret_str;
    }

    /// <summary>
    /// Returns an appropriate JSON text in accordance to the given DiffusionRequest. Factory design pattern.
    /// </summary>
    /// <param name="diffReq">given DiffusionRequest to create the JSON text from.</param>
    private string DiffusionJSONFactory(DiffusionRequest diffReq)
    {
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
            case diffusionModels.thinkdiffusiontest:
                curDiffModel = "01_ThinkDiffusionXL.safetensors";
                curImageSize = new Vector2Int(512, 512);
                break;
        }
        if (curDiffModel == null || curDiffModel == "" || curImageSize == Vector2Int.zero)
        {
            Debug.LogError("You must choose a useable Diffusion model");
            return null;
        }

        string guid = Guid.NewGuid().ToString();
        string promptText = $@"
        {{
            ""id"": ""{guid}"",
            ""prompt"": {getWorkflowJSON(diffReq.diffusionJsonType)}
        }}";

        JObject json = JObject.Parse(promptText); // promptText

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

            case diffusionWorkflows.img2img:
                StartCoroutine(UploadImage(diffReq.uploadTextures));

                json["prompt"]["10"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                json["prompt"]["3"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["3"]["inputs"]["steps"] = 10;
                break;

            case diffusionWorkflows.img2imgLCM:
                if (diffReq.uploadTextures == null)
                {
                    Debug.LogError("Upload some existing textures");
                    return null;
                }
                if (diffReq.uploadTextures.Count <= 0)
                {
                    Debug.LogError("Upload enough textures for the workflow");
                    return null;
                }

                json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["3"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;
                json["prompt"]["15"]["inputs"]["amount"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;
                
                StartCoroutine(UploadImage(diffReq.uploadTextures));

                json["prompt"]["11"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                break;

            case diffusionWorkflows.txt2img:
                /*json["prompt"]["3"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;
                json["prompt"]["5"]["inputs"]["batch_size"] = diffReq.numOfVariations;

                json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;

                json["prompt"]["5"]["inputs"]["width"] = curImageSize.x;
                json["prompt"]["5"]["inputs"]["height"] = curImageSize.y;*/
                break;

            case diffusionWorkflows.combineImages:
                if (diffReq.uploadTextures == null)
                {
                    Debug.LogError("Upload some existing textures");
                    return null;
                }
                if (diffReq.uploadTextures.Count <= 1)
                {
                    Debug.LogError("Upload enough textures for the workflow");
                    return null;
                }

                json["prompt"]["1"]["inputs"]["ckpt_name"] = curDiffModel;
                json["prompt"]["2"]["inputs"]["text"] = diffReq.positivePrompt;
                //Debug.Log("POSITIVE " + diffReq.positivePrompt);
                json["prompt"]["3"]["inputs"]["text"] = diffReq.negativePrompt;

                json["prompt"]["50"]["inputs"]["amount"] = diffReq.numOfVariations;
                json["prompt"]["51"]["inputs"]["amount"] = diffReq.numOfVariations;

                StartCoroutine(UploadImage(diffReq.uploadTextures));

                // Input Image:
                json["prompt"]["12"]["inputs"]["image"] = diffReq.uploadTextures[0].name;

                // Style is extracted from this Image:
                json["prompt"]["41"]["inputs"]["image"] = diffReq.uploadTextures[1].name;

                json["prompt"]["21"]["inputs"]["denoise"] = diffReq.denoise;
                json["prompt"]["21"]["inputs"]["seed"] = randomSeed;
                json["prompt"]["21"]["inputs"]["steps"] = 10;
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

            case diffusionWorkflows.grid4Outpainting:
            case diffusionWorkflows.outpainting:

                Debug.Log(diffReq.diffusionJsonType.ToString());
                // TODO check if words are not approved words? how to do "else" on switch statement?                
                switch (diffReq.SpecialInput)
                {
                    // Regular cases.
                    case "left":
                        // TODO ugly repeating code in these two next sections in the switch
                        if (diffReq.diffusionJsonType == diffusionWorkflows.grid4Outpainting)
                        {
                            if (diffReq.uploadTextures == null)
                            {
                                Debug.LogError("Upload textures for worklow");
                                return null;
                            }
                            if (diffReq.uploadTextures.Count <= 2)
                            {
                                Debug.LogError("Upload enough textures for worklow");
                                return null;
                            }

                            promptText = $@"
                            {{
                                ""id"": ""{guid}"",
                                ""prompt"": {getWorkflowJSON(diffusionWorkflows.grid4Outpainting)}
                            }}";

                            StartCoroutine(UploadImage(diffReq.uploadTextures));

                            json["prompt"]["80"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                            json["prompt"]["89"]["inputs"]["image"] = diffReq.uploadTextures[1].name;
                            json["prompt"]["90"]["inputs"]["image"] = diffReq.uploadTextures[2].name;
                            break;
                        }
                        else
                        {
                            StartCoroutine(UploadImage(diffReq.uploadTextures));

                            json["prompt"]["80"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                            break;
                        }                        
                    case "right":
                        if (diffReq.diffusionJsonType == diffusionWorkflows.grid4Outpainting)
                        {
                            if (diffReq.uploadTextures == null)
                            {
                                Debug.LogError("Upload textures for worklow");
                                return null;
                            }
                            if (diffReq.uploadTextures.Count <= 2)
                            {
                                Debug.LogError("Upload enough textures for worklow");
                                return null;
                            }

                            promptText = $@"
                            {{
                                ""id"": ""{guid}"",
                                ""prompt"": {getWorkflowJSON(diffusionWorkflows.grid4Outpainting)}
                            }}";

                            StartCoroutine(UploadImage(diffReq.uploadTextures));

                            json["prompt"]["80"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                            json["prompt"]["89"]["inputs"]["image"] = diffReq.uploadTextures[1].name;
                            json["prompt"]["90"]["inputs"]["image"] = diffReq.uploadTextures[2].name;

                            json["prompt"]["82"]["inputs"]["x"] = 512;
                            break;
                        }
                        else
                        {
                            StartCoroutine(UploadImage(diffReq.uploadTextures));

                            json["prompt"]["80"]["inputs"]["image"] = diffReq.uploadTextures[0].name;
                            json["prompt"]["82"]["inputs"]["x"] = 512;
                            break;
                        }
                    case "top":
                        json["prompt"]["11"]["inputs"]["top"] = 512;
                        break;                
                }


                json["prompt"]["11"]["inputs"][diffReq.SpecialInput] = 512;
                json["prompt"]["21"]["inputs"]["seed"] = randomSeed;
                /*json["prompt"]["21"]["inputs"]["denoise"] = diffReq.denoise;
                 * 
                 * VERY IMPORTANT TO GIVE PROPER PROMPTS for OUTPANGINTG
                json["prompt"]["6"]["inputs"]["text"] = diffReq.positivePrompt;
                json["prompt"]["7"]["inputs"]["text"] = diffReq.negativePrompt;*/

                // TODO needs inpainting model input?
                //json["prompt"]["4"]["inputs"]["ckpt_name"] = curDiffModel;                
                

                //Debug.Log("RIGHT " + json["prompt"]["11"]["inputs"]["right"]);

                break;

            default:
                Debug.LogError("Please choose a useable Diffusion workflow");
                return null;
        }
        return json.ToString();
    }


    /*public void RedoQueuePrompt(DiffusionRequest diffReq)
    {
        int TOTAL_TRIAL_NUMBER = 5;
        int cur = -2;

        for (int i = 0; i < TOTAL_TRIAL_NUMBER; i++)
        {
            StartCoroutine(cur = QueuePromptCoroutine(diffReq));
        }

    }*/

    /// <summary>
    /// Sends a Diffusion Image generation request to the server.
    /// </summary>
    /// <param name="diffReq">DiffusionRequest to send to the server.</param>
    public IEnumerator QueuePromptCoroutine(DiffusionRequest diffReq, int trials)
    {
        if (!readyForDiffusion || trials <= 0)
        {
            //yield return -1;
            yield break;
        }

        string url = HTTPPrefix + GameManager.getInstance().IP + "/prompt";

        string promptText = DiffusionJSONFactory(diffReq);
        while (uploadingImage)
        {
            yield return -1;
            //yield return new WaitForSeconds(0.02f);
        }

        if (promptText == null || promptText.Length <= 0)
        {
            yield return -1;
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
                GameManager.getInstance().gadget.MechanismText.text = "ERROR1 " + trials.ToString();
                Debug.Log(request.error);

                yield return new WaitForSeconds(0.2f);
                trials--;
                StartCoroutine(QueuePromptCoroutine(diffReq, trials));

                yield return 0;
            }
            else
            {
                //Debug.Log("Prompt queued successfully." + request.downloadHandler.text);

                // This is the only use of ResponseData, but it is needed for proper downloading of the prompt
                ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
                diffReq.prompt_id = data.prompt_id;
                yield return 1;
            }                
        }
    }


    /*// Used to save up on compute when not using the image generation
    private IEnumerator SmallWait()
    {
        yield return new WaitForSeconds(5f);
    }

    private async void StartListening()
    {
        //var buffer = new byte[1024 * 1024 * 32];
        //WebSocketReceiveResult result = null;

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
    }*/

    void OnDestroy()
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }


    // TODO use the same technique with checking the output/outputs(?) folder before sending a single(?) download request
    public IEnumerator CheckIfFileExists(string imageName, FileExistsChecker fileChecker, bool uploadImageStatusAtEnd)
    {
        string url = HTTPPrefix + GameManager.getInstance().IP + "/view?filename=" + imageName + "&type=input";

        using (var unityWebRequest = UnityWebRequest.Head(url))
        {
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.result != UnityWebRequest.Result.Success)
            {
                uploadingImage = false;
                Debug.Log("File " + imageName + " still not in Input");
            }
            else
            {
                uploadingImage = uploadImageStatusAtEnd;
                fileChecker.fileExists = true;
                Debug.Log("File " + imageName + " in Input");
            }
        }

        /*using (UnityWebRequest request = UnityWebRequest.Head(url))
        {
            // Send the request and wait for a response
            yield return request.SendWebRequest();

            // Check for network errors
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else
            {
                // Check the response code
                if (request.responseCode == 200)
                {
                    Debug.Log("File exists.");
                    fileChecker.fileExists = true;
                }
                else
                {
                    Debug.Log("File does not exist or is inaccessible.");
                }
            }
        }*/
    }


    public void RequestFileName(DiffusionRequest diffReq)
    {
        StartCoroutine(RequestFileNameRoutine(diffReq));
    }

    /// <summary>
    /// Requests and downloads the images created for a given DiffusionRequest
    /// </summary>
    /// <param name="diffReq">given DiffusionRequest to download the images created for it</param>
    IEnumerator RequestFileNameRoutine(DiffusionRequest diffReq)
    {
        string url = HTTPPrefix + GameManager.getInstance().IP + "/history/" + diffReq.prompt_id;

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
                    GameManager.getInstance().gadget.MechanismText.text = "PROTERR";
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

                    /*Debug.Log("All File Names:");
                    foreach (string item in filenames) { 
                        Debug.Log(item);
                    }*/

                    // If there are no filenames, the prompt has not yet finished generating
                    if (filenames.Length <= 0)
                    {
                        break;
                    }

                    // Downloading each image of the prompt
                    for (int i = 0; i < filenames.Length; i++)
                    {
                        string imageURL = HTTPPrefix + GameManager.getInstance().IP + "/view?filename=" + filenames[i];
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

    /// <summary>
    /// Downloads a single image according to the given image URL and adds it to the DiffusionRequest
    /// </summary>
    /// <param name="imageUrl">Image URL to download</param>
    /// <param name="diffReq">DiffusionRequest to add downloaded image to</param>
    IEnumerator DownloadImage(string imageUrl, DiffusionRequest diffReq)
    {
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

    // TODO remove uploadImageStatusAtEnd

    /// <summary>
    /// Uploads a given Texture to the server.
    /// </summary>
    /// <param name="curTexture">Texture to upload to the server.</param>
    /// <param name="uploadImageStatusAtEnd">Sets the final status of the uploadingImage parameter at the end of the function's work.</param>
    private IEnumerator UploadImage(List<Texture2D> curTextures, bool uploadImageStatusAtEnd = false)
    {
        foreach (Texture2D curTexture in curTextures)
        {
            string url = HTTPPrefix + GameManager.getInstance().IP + "/upload/image";

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
                    /*Debug.Log("Image Upload succesful");
                    Debug.Log(unityWebRequest.uploadHandler.progress);*/

                    FileExistsChecker fileCheck = new FileExistsChecker();
                    while (!fileCheck.fileExists)
                    {
                        yield return new WaitForSeconds(0.2f);
                        StartCoroutine(CheckIfFileExists(curTexture.name, fileCheck, uploadImageStatusAtEnd));
                    }
                }
            }
        }
    }
}
