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

[System.Serializable]
public class ResponseData
{
    public string prompt_id;
}
public class ResponseDataWebsocket
{
    public string prompt_id;
}

[System.Serializable]
public class ImageData
{
    public string filename;
    public string subfolder;
    public string type;
}

[System.Serializable]
public class OutputData
{
    public ImageData[] images;
}

[System.Serializable]
public class PromptData
{
    public OutputData outputs;
}


public class ComfySceneLibrary : MonoBehaviour
{
    private static int TOTAL_NUM_TEXTURES = 50;
    private Texture2D[] textures = new Texture2D[TOTAL_NUM_TEXTURES];
    private Sprite[] final_sprites = new Sprite[TOTAL_NUM_TEXTURES];

    private string serverAddress = "127.0.0.1:8188";
    private string clientId = Guid.NewGuid().ToString();
    private ClientWebSocket ws = new ClientWebSocket();

    public GameObject blocks;
    private GameObject[] children_blocks;

    private int num_texture = 0;
    //private System.Timers.Timer texture_timer = new System.Timers.Timer(1000);

    public string positivePrompt;
    public string negativePrompt;
    public TextAsset promptJson;

    private async void Start()
    {
        await ws.ConnectAsync(new Uri($"ws://{serverAddress}/ws?clientId={clientId}"), CancellationToken.None);
        StartListening();
    }
    
    public void PromptActivate()
    {
        StartCoroutine(QueuePromptCoroutine());
    }

    public IEnumerator QueuePromptCoroutine()
    {
        string url = "http://" + serverAddress + "/prompt";

        string guid = Guid.NewGuid().ToString();
        string promptText = $@"
        {{
            ""id"": ""{guid}"",
            ""prompt"": {promptJson.text}
        }}";

        // Replacing stand-in tags with relevant input for the final generation
        promptText = promptText.Replace("Pprompt", positivePrompt);
        promptText = promptText.Replace("Nprompt", negativePrompt);
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
            Debug.Log("Prompt queued successfully." + request.downloadHandler.text);

            ResponseData data = JsonUtility.FromJson<ResponseData>(request.downloadHandler.text);
            Debug.Log("Prompt ID: " + data.prompt_id);
            GetComponent<ComfyWebsocket>().promptID = data.prompt_id;
            // GetComponent<ComfyImageCtr>().RequestFileName(data.prompt_id);
        }

        yield break;
    }

    public string promptID;
    private async void StartListening()
    {
        //new byte[1024 * 4];
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
                Debug.Log("RESULT COUNT:" + result.Count.ToString());
            }
            while (!result.EndOfMessage);

            string response = stringBuilder.ToString();
            Debug.Log("Received: " + response);

            if (response.Contains("\"queue_remaining\": 0"))
            {
                RequestFileName(promptID);
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
                    Debug.LogError(": HTTP Error: " + webRequest.error);
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
                    Debug.Log("FILENAMES: " + result);



                    for (int i = 0; i < filenames.Length; i++)
                    {
                        string imageURL = "http://" + serverAddress + "/view?filename=" + filenames[i];
                        Debug.Log(filenames[i]);
                        //StartCoroutine(ExampleCoroutine());
                        StartCoroutine(DownloadImage(imageURL));
                    }
                    InvokeRepeating("DelayedChangeToTexture", 1f, 0.01f);
                    break;
            }
        }
    }

    IEnumerator ExampleCoroutine()
    {
        //Print the time of when the function is first called.
        //Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1f);

        //After we have waited 5 seconds print the time again.
        //Debug.Log("Finished Coroutine at timestamp : " + Time.time);
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

        Debug.Log(filenames);
        return filenames;
    }

    IEnumerator DownloadImage(string imageUrl)
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
                AddTextureToTotal(texture);
            }
            else
            {
                Debug.LogError("Image download failed: " + webRequest.error);
            }
        }
    }

    void AddTextureToTotal(Texture2D texture)
    {
        textures[num_texture] = texture;
        num_texture++;
        if (num_texture >= TOTAL_NUM_TEXTURES)
        {
            num_texture = 0;

            for (int i = 0; i < TOTAL_NUM_TEXTURES; i++)
            {
                Texture2D cur_texture = textures[i];
                final_sprites[i] = Sprite.Create(cur_texture, new Rect(0, 0, cur_texture.width, cur_texture.height), Vector2.zero);
            }

            children_blocks = new GameObject[blocks.transform.childCount];
            for (int i = 0; i < blocks.transform.childCount; i++)
            {
                children_blocks[i] = blocks.transform.GetChild(i).gameObject;
            }
        }
    }

    void DelayedChangeToTexture()
    {
        if (final_sprites == null | blocks == null)
        {
            return;
        }

        Sprite cur_sprite = final_sprites[UnityEngine.Random.Range(0, TOTAL_NUM_TEXTURES)];

        Debug.Log("COUNT: " + blocks.transform.childCount.ToString());
        GameObject cur_child_block = children_blocks[UnityEngine.Random.Range(0, blocks.transform.childCount)];
        if (cur_child_block != null)
        {
            Renderer cur_block_renderer = cur_child_block.GetComponent<Renderer>();
            Texture2D cur_texture = textures[UnityEngine.Random.Range(0, TOTAL_NUM_TEXTURES)];
            cur_block_renderer.material.SetTexture("_MainTex", cur_texture);
        }
    }
}
