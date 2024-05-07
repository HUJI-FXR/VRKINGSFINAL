using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class CrossFade : MonoBehaviour
{
    private Renderer renderer;
    private float duration = 5f;
    private float changeTextureEvery = 5f;

    public Texture[] textures;
    private int texNum = 1;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        renderer.material.SetFloat("_Blend", 0f);

        InvokeRepeating("CrossFadeStart", changeTextureEvery, 1f);
    }

    void Update()
    {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        renderer.material.SetFloat("_Blend", lerp);
    }

    public void CrossFadeStart()
    {
        CrossFadeBetween(textures[texNum-1], textures[texNum]);

        texNum++;
        if (texNum >= textures.Length) {
            texNum = 1;
        }
    }

    private void CrossFadeBetween(Texture beforeTexture, Texture afterTexture)
    {
        renderer.material.SetTexture("_MainTex", beforeTexture);
        renderer.material.SetTexture("_TransitionTex", afterTexture);
    }
}