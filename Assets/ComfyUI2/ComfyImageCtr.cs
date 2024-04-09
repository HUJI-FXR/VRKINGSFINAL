using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.Windows;
using System.Text;
using System.Timers;
using System.Threading;

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

public class ComfyImageCtr: MonoBehaviour
{
    Queue<Texture2D> textures = new Queue<Texture2D>();
    System.Timers.Timer texture_timer = new System.Timers.Timer(1000);
    private void Start()
    {
        //texture_timer.Elapsed += TimerElapsed;
        texture_timer.AutoReset = true; // AutoReset is set to true to restart the timer automatically
        texture_timer.Enabled = true;
    }
    public void RequestFileName(string id){
    StartCoroutine(RequestFileNameRoutine(id));
}

 IEnumerator RequestFileNameRoutine(string promptID)
    {
        string url = "http://127.0.0.1:8188/history/" + promptID;
        
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
                    //Debug.Log("Got URL: " + url);
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
                        string imageURL = "http://127.0.0.1:8188/view?filename=" + filenames[i];
                        StartCoroutine(DownloadImage(imageURL));
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
            int startIndex = jsonString.IndexOf(keyToLookFor, prevIndex+1);
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

    public Image outputImage;
    
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

                outputImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    
            }
            else
            {
                Debug.LogError("Image download failed: " + webRequest.error);
            }
        }
    }

    // todo want to cause texture change every 0.1 seconds without regard of what is happening around it
    async void DelayedChangeToTexture(Texture2D texture)
    {
        textures.Enqueue(texture);
        texture_timer.Start();

        //if (texture_timer.Elapsed)
        textures.Dequeue();
    }
}
