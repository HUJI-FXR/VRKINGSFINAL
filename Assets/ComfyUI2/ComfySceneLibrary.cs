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

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}
/*public class ResponseDataWebsocket
{
    public string prompt_id;
}*/

/*[System.Serializable]
public class ImageData
{
    public string filename;
    public string subfolder;
    public string type;
}*/

/*[System.Serializable]
public class OutputData
{
    public ImageData[] images;
}*/

/*[System.Serializable]
public class PromptData
{
    public OutputData outputs;
}*/

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
    public GameObject[] childrenBlocks;
}

public class ComfySceneLibrary : MonoBehaviour
{
    public GameObjectPromptJsonPair[] TextureLists;

    private string serverAddress = "127.0.0.1:8188";
    private string clientId = Guid.NewGuid().ToString();
    private ClientWebSocket ws = new ClientWebSocket();

    private string promptID;
    private int curParentObject = 0;

    private bool started_generations = false;

    private async void Start()
    {
        for (int i = 0; i<TextureLists.Length; i++)
        {
            TextureLists[i].textures = new List<Texture2D>();

            Transform curParentTransform = TextureLists[i].ParentObject.transform;
            int numberOfChildren = curParentTransform.childCount;
            if (numberOfChildren > 0)
            {
                // TODO what if the number of children changes in the middle of the game inside a parent object? need to FIX
                TextureLists[i].childrenBlocks = new GameObject[numberOfChildren];
                for (int j = 0; j < numberOfChildren; j++)
                {
                    TextureLists[i].childrenBlocks[j] = curParentTransform.GetChild(j).gameObject;
                }
            }
        }

        InvokeRepeating("DelayedChangeToTexture", 1f, 0.01f);

        await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);
        StartListening();
    }

    public void PromptActivate(InputAction.CallbackContext context)
    {
        started_generations = true;
        for (int i = 0;i<TextureLists.Length;i++)
        {
            if (TextureLists[i].active && context.performed)
            {
                Debug.Log("PRMPT " + TextureLists.Length);
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
            promptID = data.prompt_id;
            //Debug.Log("Prompt ID: " + data.prompt_id);
        }

        yield break;
    }

    private async void StartListening()
    {
        var buffer = new byte[1024 * 16];
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
            //Debug.Log("Received: " + response);

            if (response.Contains("\"queue_remaining\": 0"))
            {
                if (TextureLists[curParentObject].active)
                {
                    RequestFileName(promptID, curParentObject);
                }
                
                curParentObject++;
                if (curParentObject >= TextureLists.Length)
                {
                    curParentObject = 0;
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

    public void RequestFileName(string id, int curGroup)
    {
        StartCoroutine(RequestFileNameRoutine(id, curGroup));
    }

    IEnumerator RequestFileNameRoutine(string promptID, int curGroup)
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
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    // Jonathan - added the for loop to coincide with the changes to the ExtractFilename function becoming a batch-size dependant downloader
                    // Jonathan - another change, download all the images FIRST, then display, to test display speed
                    string[] filenames = ExtractFilename(webRequest.downloadHandler.text);

                    // Print all filenames
                    StringBuilder sb = new StringBuilder();
                    foreach (string filename in filenames)
                    {
                        sb.Append(filename);
                        sb.Append(" ");
                    }
                    string result = sb.ToString();

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

        //Debug.Log(filenames);
        return filenames;
    }

    IEnumerator DownloadImage(string imageUrl, int curGroup)
    {
        //yield return new WaitForSeconds(0.5f);
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

    void DelayedChangeToTexture()
    {
        for (int i = 0; i < TextureLists.Length; i++)
        {
            GameObject cur_child_block = TextureLists[i].childrenBlocks[UnityEngine.Random.Range(0, TextureLists[i].childrenBlocks.Length)];
            if (cur_child_block != null & TextureLists[i].textures != null)
            {
                if (TextureLists[i].textures.Count <= 0)
                {
                    continue;
                }
                Material cur_child_mat = cur_child_block.GetComponent<Material>();
                if (cur_child_mat != null)
                {
                    Debug.Log(cur_child_mat.name);
                }
                /*if (cur_child_mat != null && cur_child_mat.name != "TransMaterial") {
                    Renderer cur_block_renderer = cur_child_block.GetComponent<Renderer>();
                    Texture2D cur_texture = TextureLists[i].textures[UnityEngine.Random.Range(0, TextureLists[i].textures.Count)];
                    //cur_child_mat.shader.SetGlobalTexture("_tex1", cur_texture);
                    cur_block_renderer.material.SetTexture("_tex1", cur_texture);

                    Debug.Log("HAPPENED");
                }
                else
                {*/
                Renderer cur_block_renderer = cur_child_block.GetComponent<Renderer>();
                Texture2D cur_texture = TextureLists[i].textures[UnityEngine.Random.Range(0, TextureLists[i].textures.Count)];
                cur_block_renderer.material.SetTexture("_MainTex", cur_texture);
                /*}*/
            }
        }
    }
}
