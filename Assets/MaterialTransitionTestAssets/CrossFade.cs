using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class CrossFade : MonoBehaviour
{
    public Texture[] textures;

    private Renderer renderer;
    private float duration = 2f;
    private int texNum = 0;
    private float lerp = 0f;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        renderer.material.SetFloat("_Blend", 0f);
    }

    void Update()
    {
        lerp += Time.deltaTime / duration;
        renderer.material.SetFloat("_Blend", lerp);

        if (lerp > 1)
        {
            CrossFadeStart();
        }
    }

    public void CrossFadeStart()
    {
        lerp = 0;

        texNum++;
        if (texNum < textures.Length) {
            CrossFadeBetween(textures[texNum - 1], textures[texNum]);
        }
        else
        {
            texNum = 0;
            CrossFadeBetween(textures[textures.Length-1], textures[texNum]);
        }
    }

    private void CrossFadeBetween(Texture beforeTexture, Texture afterTexture)
    {
        renderer.material.SetTexture("_MainTex", beforeTexture);
        renderer.material.SetTexture("_TransitionTex", afterTexture);
    }
}