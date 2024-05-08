using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using UnityEditor.PackageManager;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.TextCore.Text;
using UnityEngine.Rendering;
using UnityEditor.PackageManager.Requests;
using System.Linq;

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}


[Serializable]
public struct GameObjectPromptJsonPair
{
    public GameObject ParentObject;

    public string positivePrompt;
    public string negativePrompt;
    public UnityEngine.TextAsset promptJson;

    public bool active;

    [System.NonSerialized]
    public List<Texture2D> textures;
    [System.NonSerialized]
    public Dictionary<string, bool> genPromptIDs;
}

public class ComfySceneLibrary : MonoBehaviour
{
    public GameObjectPromptJsonPair[] TextureLists;

    private string serverAddress = "jonathanmiroshnik-glitch-08211026.thinkdiffusion.xyz";  //"127.0.0.1:8188";
    private string clientId = Guid.NewGuid().ToString();
    private ClientWebSocket ws = new ClientWebSocket();

    private bool started_generations = false;

    private async void Start()
    {
        for (int i = 0; i<TextureLists.Length; i++)
        {
            TextureLists[i].textures = new List<Texture2D>();
            TextureLists[i].genPromptIDs = new Dictionary<string, bool>();
        }

        InvokeRepeating("DelayedChangeToTexture", 1f, 0.01f);

        //await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);

        //www.thinkdiffusion.com/sd/jonathanmiroshnik-dreambooth-08205736.thinkdiffusion.xyz
        await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);
        StartListening();

        ButtonTest();
    }

    public void PromptActivate(InputAction.CallbackContext context)
    {
        started_generations = true;
        for (int i = 0;i<TextureLists.Length;i++)
        {
            if (TextureLists[i].active && context.performed)
            {
                StartCoroutine(QueuePromptCoroutine(i));
            }
        }
    }

    public void ButtonTest()
    {
        started_generations = true;
        for (int i = 0; i < TextureLists.Length; i++)
        {
            if (TextureLists[i].active)
            {
                StartCoroutine(QueuePromptCoroutine(i));
            }
        }
    }

    private IEnumerator QueuePromptCoroutine(int curGroup)
    {
        string url = "http://" + serverAddress + "/prompt";

        string guid = Guid.NewGuid().ToString();
        string promptText = $@"
        {{
            ""id"": ""{guid}"",
            ""prompt"": {TextureLists[curGroup].promptJson.text}
        }}";

        // Replacing stand-in tags with relevant input for the final generation
        promptText = promptText.Replace("Pprompt", TextureLists[curGroup].positivePrompt);
        promptText = promptText.Replace("Nprompt", TextureLists[curGroup].negativePrompt);
        promptText = promptText.Replace("SeedHere", UnityEngine.Random.Range(1, 10000).ToString());

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
            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            TextureLists[curGroup].genPromptIDs.TryAdd(data.prompt_id, false);
        }

        yield break;
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

            // Goes over each prompt that needs completing and checks whether it has completed, if it did, it downloads the images and labels the promptID as true(as in, finished)
            for (int i = 0; i < TextureLists.Length; i++)
            {
                string[] cur_arr_keys = TextureLists[i].genPromptIDs.Keys.ToArray<string>();
                for (int j = 0; j < cur_arr_keys.Length; j++)
                {
                    if (TextureLists[i].genPromptIDs[cur_arr_keys[j]])
                    {
                        break;
                    }

                    // TODO this solution requires us to constantly ask the server about each promptID and whether it has finished it - inefficient!
                    // TODO - cont. can we not check which promptID has finished without asking to download it?
                    RequestFileName(cur_arr_keys[j]);
                }
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

    public void RequestFileName(string id)
    {
        StartCoroutine(RequestFileNameRoutine(id));
    }

    IEnumerator RequestFileNameRoutine(string promptID)
    {
        string url = "http://" + serverAddress + "/history/" + promptID;
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

                    // If there are no filenames, the prompt has not yet finished generating
                    if (filenames.Length <= 0)
                    {
                        break;
                    }

                    // Print all filenames
                    /*StringBuilder sb = new StringBuilder();
                    foreach (string filename in filenames)
                    {
                        sb.Append(filename);
                        sb.Append(" ");
                    }
                    string result = sb.ToString();*/

                    // Checks which TextureLists group has asked for the prompt
                    int curGroup = -1;
                    for (int textureListGroup = 0; textureListGroup < TextureLists.Length; textureListGroup++) {
                        if (TextureLists[textureListGroup].genPromptIDs.Keys.Contains(promptID))
                        {
                            curGroup = textureListGroup;
                            break;
                        }
                    }
                    if (curGroup < 0)
                    {
                        break;
                    }

                    // Indicating that this prompt has been completed(and won't be repeated with the same exact PromptID and is now downloading)
                    TextureLists[curGroup].genPromptIDs[promptID] = true;

                    // Downloading each image of the prompt
                    for (int i = 0; i < filenames.Length; i++)
                    { 
                        string imageURL = "http://" + serverAddress + "/view?filename=" + filenames[i];
                        StartCoroutine(DownloadImage(imageURL, curGroup));
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
                // Jonathan - changed to null because we return string[] instead of string in the func from now on
                return null;
                //return "filename key not found";
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

    IEnumerator DownloadImage(string imageUrl, int curGroup)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Get the downloaded texture
                Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;
                // Adding the texture to the texture queue
                TextureLists[curGroup].textures.Add(texture);
            }
            else
            {
                Debug.LogError("Image download failed: " + webRequest.error);
            }
        }
    }

    // TODO Change of texture functionality is DIFFERENT and should happen in a DIFFERENT place than creating and downloading he imagery
    void DelayedChangeToTexture()
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
    }
}
