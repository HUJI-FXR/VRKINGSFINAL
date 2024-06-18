using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class ExternalCameraDepth : MonoBehaviour
{
    [NotNull] [SerializeField] private RenderTexture _renderTexture;
    [NotNull] [SerializeField] private GameObject externalCamera;

    private Texture2D _texture2D;

    [SerializeField] private Renderer project;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SaveTexture();
    }

    public void SaveTexture()
    {
        externalCamera.SetActive(true);
        _texture2D = toTexture2D(_renderTexture);
        externalCamera.SetActive(false);
        

        if (project != null)
        {
            project.material.mainTexture = _texture2D;
        }
    }
    
    public Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        var old_rt = RenderTexture.active;
        RenderTexture.active = rTex;

        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        RenderTexture.active = old_rt;
        return tex;
    }
}
