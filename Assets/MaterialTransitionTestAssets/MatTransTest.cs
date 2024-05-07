using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class MatTransTest : MonoBehaviour
{
    // Blends between two materials

    public Material material1;
    public Material material2;
    public float duration = 2.0f;

    private Renderer rend;
    //private List<Material> mats = new List<Material>();

    void Start()
    {
        rend = gameObject.GetComponent<Renderer>();

        // At start, use the first material
        rend.material = material1;

        //mats.Add(material1);
        //mats.Add(material2);
    }

    void Update()
    {
        // ping-pong between the materials over the duration
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        //Debug.Log(lerp);
        //rend.material.Lerp(material1, material2, 1000);

        rend.material.SetFloat("_Cutoff", lerp);

        Texture tex1 = material1.mainTexture;
        Texture tex2 = material2.mainTexture;

        RenderTexture rt1 = null;
        RenderTexture rt2 = null;

        Graphics.Blit(tex1, rt1);
        Graphics.Blit(tex2, rt2);

        Graphics.Blit(rt1, rt2, rend.material);


        //rend.material 
        //Graphics.Blit()
        //rend.material.Lerp(material1, material2, lerp);
        //rend.SetMaterials(mats);
        //rend.material.SetColor("def", Color.Lerp(material1.color, material2.color, lerp));
    }
}
