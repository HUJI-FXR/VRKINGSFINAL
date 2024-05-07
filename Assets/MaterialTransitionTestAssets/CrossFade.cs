using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;
using System.Collections.Generic;

public class CrossFade : MonoBehaviour
{
    public List<Texture> textures;

    private Renderer renderer;
    private float duration = 1f;
    private int texNum = 0;
    private float lerp = 0f;

    void Start()
    {
        textures = new List<Texture>();

        renderer = GetComponent<Renderer>();
        renderer.material.SetFloat("_Blend", 0f);
    }

    void Update()
    {
        if (textures.Count > 0)
        {
            lerp += Time.deltaTime / duration;
            renderer.material.SetFloat("_Blend", lerp);

            if (lerp > 1)
            {
                CrossFadeStart();
            }
        }
    }

    public void CrossFadeStart()
    {
        lerp = 0;

        texNum++;
        if (texNum < textures.Count) {
            CrossFadeBetween(textures[texNum - 1], textures[texNum]);
        }
        else
        {
            texNum = 0;
            CrossFadeBetween(textures[textures.Count-1], textures[texNum]);
        }
    }

    private void CrossFadeBetween(Texture beforeTexture, Texture afterTexture)
    {
        renderer.material.SetTexture("_MainTex", beforeTexture);
        renderer.material.SetTexture("_TransitionTex", afterTexture);
    }
}