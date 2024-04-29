using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices.WindowsRuntime;

//[System.Serializable]
//public class ResponseData
//{
//    public string prompt_id;
//}
public class ComfyPromptCtr : MonoBehaviour
{

    public InputField pInput,nInput,promptJsonInput;

    private string[] Pprompts = new string[3];
    private string[] comfyUIJsons = new string[3];


    private void Start()
    {
        Pprompts[0] = "very red brick wall texture, game texture";
        Pprompts[1] = "very blue beautiful buttrflies flying inside a wall";
        Pprompts[2] = "very yellow balls of light emerging from a wall";

        comfyUIJsons[0] = "{\r\n  \"3\": {\r\n    \"inputs\": {\r\n      \"seed\": \"SeedHere\",\r\n      \"steps\": 3,\r\n      \"cfg\": 1.5,\r\n      \"sampler_name\": \"euler\",\r\n      \"scheduler\": \"normal\",\r\n      \"denoise\": 0.6,\r\n      \"model\": [\r\n        \"10\",\r\n        0\r\n      ],\r\n      \"positive\": [\r\n        \"6\",\r\n        0\r\n      ],\r\n      \"negative\": [\r\n        \"7\",\r\n        0\r\n      ],\r\n      \"latent_image\": [\r\n        \"14\",\r\n        0\r\n      ]\r\n    },\r\n    \"class_type\": \"KSampler\",\r\n    \"_meta\": {\r\n      \"title\": \"KSampler\"\r\n    }\r\n  },\r\n  \"4\": {\r\n    \"inputs\": {\r\n      \"ckpt_name\": \"stable-diffusion-nano-2-1.ckpt\"\r\n    },\r\n    \"class_type\": \"CheckpointLoaderSimple\",\r\n    \"_meta\": {\r\n      \"title\": \"Load Checkpoint\"\r\n    }\r\n  },\r\n  \"5\": {\r\n    \"inputs\": {\r\n      \"width\": 128,\r\n      \"height\": 128,\r\n      \"batch_size\": 50\r\n    },\r\n    \"class_type\": \"EmptyLatentImage\",\r\n    \"_meta\": {\r\n      \"title\": \"Empty Latent Image\"\r\n    }\r\n  },\r\n  \"6\": {\r\n    \"inputs\": {\r\n      \"text\": \"Pprompt\",\r\n      \"clip\": [\r\n        \"10\",\r\n        1\r\n      ]\r\n    },\r\n    \"class_type\": \"CLIPTextEncode\",\r\n    \"_meta\": {\r\n      \"title\": \"CLIP Text Encode (Prompt)\"\r\n    }\r\n  },\r\n  \"7\": {\r\n    \"inputs\": {\r\n      \"text\": \"Nprompt\",\r\n      \"clip\": [\r\n        \"10\",\r\n        1\r\n      ]\r\n    },\r\n    \"class_type\": \"CLIPTextEncode\",\r\n    \"_meta\": {\r\n      \"title\": \"CLIP Text Encode (Prompt)\"\r\n    }\r\n  },\r\n  \"8\": {\r\n    \"inputs\": {\r\n      \"samples\": [\r\n        \"3\",\r\n        0\r\n      ],\r\n      \"vae\": [\r\n        \"4\",\r\n        2\r\n      ]\r\n    },\r\n    \"class_type\": \"VAEDecode\",\r\n    \"_meta\": {\r\n      \"title\": \"VAE Decode\"\r\n    }\r\n  },\r\n  \"9\": {\r\n    \"inputs\": {\r\n      \"filename_prefix\": \"ComfyUI\",\r\n      \"images\": [\r\n        \"8\",\r\n        0\r\n      ]\r\n    },\r\n    \"class_type\": \"SaveImage\",\r\n    \"_meta\": {\r\n      \"title\": \"Save Image\"\r\n    }\r\n  },\r\n  \"10\": {\r\n    \"inputs\": {\r\n      \"lora_name\": \"pytorch_lora_weights.safetensors\",\r\n      \"strength_model\": 1,\r\n      \"strength_clip\": 1,\r\n      \"model\": [\r\n        \"4\",\r\n        0\r\n      ],\r\n      \"clip\": [\r\n        \"4\",\r\n        1\r\n      ]\r\n    },\r\n    \"class_type\": \"LoraLoader\",\r\n    \"_meta\": {\r\n      \"title\": \"Load LoRA\"\r\n    }\r\n  },\r\n  \"11\": {\r\n    \"inputs\": {\r\n      \"image\": \"Brick-Texture-900317381 (2).jpg\",\r\n      \"upload\": \"image\"\r\n    },\r\n    \"class_type\": \"LoadImage\",\r\n    \"_meta\": {\r\n      \"title\": \"Load Image\"\r\n    }\r\n  },\r\n  \"12\": {\r\n    \"inputs\": {\r\n      \"pixels\": [\r\n        \"11\",\r\n        0\r\n      ],\r\n      \"vae\": [\r\n        \"4\",\r\n        2\r\n      ]\r\n    },\r\n    \"class_type\": \"VAEEncode\",\r\n    \"_meta\": {\r\n      \"title\": \"VAE Encode\"\r\n    }\r\n  },\r\n  \"14\": {\r\n    \"inputs\": {\r\n      \"amount\": 50,\r\n      \"samples\": [\r\n        \"12\",\r\n        0\r\n      ]\r\n    },\r\n    \"class_type\": \"RepeatLatentBatch\",\r\n    \"_meta\": {\r\n      \"title\": \"Repeat Latent Batch\"\r\n    }\r\n  }\r\n}";
        comfyUIJsons[1] = "{\r\n  \"3\": {\r\n    \"inputs\": {\r\n      \"seed\": \"SeedHere\",\r\n      \"steps\": 3,\r\n      \"cfg\": 1.5,\r\n      \"sampler_name\": \"euler\",\r\n      \"scheduler\": \"normal\",\r\n      \"denoise\": 0.6,\r\n      \"model\": [\r\n        \"10\",\r\n        0\r\n      ],\r\n      \"positive\": [\r\n        \"6\",\r\n        0\r\n      ],\r\n      \"negative\": [\r\n        \"7\",\r\n        0\r\n      ],\r\n      \"latent_image\": [\r\n        \"14\",\r\n        0\r\n      ]\r\n    },\r\n    \"class_type\": \"KSampler\",\r\n    \"_meta\": {\r\n      \"title\": \"KSampler\"\r\n    }\r\n  },\r\n  \"4\": {\r\n    \"inputs\": {\r\n      \"ckpt_name\": \"stable-diffusion-nano-2-1.ckpt\"\r\n    },\r\n    \"class_type\": \"CheckpointLoaderSimple\",\r\n    \"_meta\": {\r\n      \"title\": \"Load Checkpoint\"\r\n    }\r\n  },\r\n  \"5\": {\r\n    \"inputs\": {\r\n      \"width\": 128,\r\n      \"height\": 128,\r\n      \"batch_size\": 50\r\n    },\r\n    \"class_type\": \"EmptyLatentImage\",\r\n    \"_meta\": {\r\n      \"title\": \"Empty Latent Image\"\r\n    }\r\n  },\r\n  \"6\": {\r\n    \"inputs\": {\r\n      \"text\": \"Pprompt\",\r\n      \"clip\": [\r\n        \"10\",\r\n        1\r\n      ]\r\n    },\r\n    \"class_type\": \"CLIPTextEncode\",\r\n    \"_meta\": {\r\n      \"title\": \"CLIP Text Encode (Prompt)\"\r\n    }\r\n  },\r\n  \"7\": {\r\n    \"inputs\": {\r\n      \"text\": \"Nprompt\",\r\n      \"clip\": [\r\n        \"10\",\r\n        1\r\n      ]\r\n    },\r\n    \"class_type\": \"CLIPTextEncode\",\r\n    \"_meta\": {\r\n      \"title\": \"CLIP Text Encode (Prompt)\"\r\n    }\r\n  },\r\n  \"8\": {\r\n    \"inputs\": {\r\n      \"samples\": [\r\n        \"3\",\r\n        0\r\n      ],\r\n      \"vae\": [\r\n        \"4\",\r\n        2\r\n      ]\r\n    },\r\n    \"class_type\": \"VAEDecode\",\r\n    \"_meta\": {\r\n      \"title\": \"VAE Decode\"\r\n    }\r\n  },\r\n  \"9\": {\r\n    \"inputs\": {\r\n      \"filename_prefix\": \"ComfyUI\",\r\n      \"images\": [\r\n        \"8\",\r\n        0\r\n      ]\r\n    },\r\n    \"class_type\": \"SaveImage\",\r\n    \"_meta\": {\r\n      \"title\": \"Save Image\"\r\n    }\r\n  },\r\n  \"10\": {\r\n    \"inputs\": {\r\n      \"lora_name\": \"pytorch_lora_weights.safetensors\",\r\n      \"strength_model\": 1,\r\n      \"strength_clip\": 1,\r\n      \"model\": [\r\n        \"4\",\r\n        0\r\n      ],\r\n      \"clip\": [\r\n        \"4\",\r\n        1\r\n      ]\r\n    },\r\n    \"class_type\": \"LoraLoader\",\r\n    \"_meta\": {\r\n      \"title\": \"Load LoRA\"\r\n    }\r\n  },\r\n  \"11\": {\r\n    \"inputs\": {\r\n      \"image\": \"chocolate-ice-cream-sugar-cone-5141161-1276149850.jpg\",\r\n      \"upload\": \"image\"\r\n    },\r\n    \"class_type\": \"LoadImage\",\r\n    \"_meta\": {\r\n      \"title\": \"Load Image\"\r\n    }\r\n  },\r\n  \"12\": {\r\n    \"inputs\": {\r\n      \"pixels\": [\r\n        \"11\",\r\n        0\r\n      ],\r\n      \"vae\": [\r\n        \"4\",\r\n        2\r\n      ]\r\n    },\r\n    \"class_type\": \"VAEEncode\",\r\n    \"_meta\": {\r\n      \"title\": \"VAE Encode\"\r\n    }\r\n  },\r\n  \"14\": {\r\n    \"inputs\": {\r\n      \"amount\": 50,\r\n      \"samples\": [\r\n        \"12\",\r\n        0\r\n      ]\r\n    },\r\n    \"class_type\": \"RepeatLatentBatch\",\r\n    \"_meta\": {\r\n      \"title\": \"Repeat Latent Batch\"\r\n    }\r\n  }\r\n}";
        comfyUIJsons[2] = "";

        //QueuePrompt();
        //InvokeRepeating("QueuePrompt", 1f, 7);
    }

    public void BrickTest()
    {
        promptJson = comfyUIJsons[0];
        StartCoroutine(QueuePromptCoroutine(Pprompts[0], "watermark, bad quality"));
    }

    public void IcecreamTest()
    {
        promptJson = comfyUIJsons[1];
        StartCoroutine(QueuePromptCoroutine(Pprompts[1], "watermark, bad quality"));
    }

    public void QueuePrompt()
    {
        //pInput.text,nInput.text
        StartCoroutine(QueuePromptCoroutine(Pprompts[UnityEngine.Random.Range(0, Pprompts.Length)], "watermark, bad quality"));
    }
    private IEnumerator QueuePromptCoroutine(string positivePrompt,string negativePrompt)
    {
        string url = "http://127.0.0.1:8188/prompt";
        string promptText = GeneratePromptJson();
        promptText = promptText.Replace("Pprompt", positivePrompt);
        promptText = promptText.Replace("Nprompt", negativePrompt);

        Debug.Log(promptText);

        //UnityEngine.Random.Range(1, 1000).ToString()
        promptText = promptText.Replace("SeedHere", UnityEngine.Random.Range(1, 1000).ToString());

        //Debug.Log(promptText);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(promptText);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            //Debug.Log(request.error);
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

    public string promptJson;

private string GeneratePromptJson()
    {
 string guid = Guid.NewGuid().ToString();

    string promptJsonWithGuid = $@"
{{
    ""id"": ""{guid}"",
    ""prompt"": {promptJson}
}}";

    return promptJsonWithGuid;
    }
}
