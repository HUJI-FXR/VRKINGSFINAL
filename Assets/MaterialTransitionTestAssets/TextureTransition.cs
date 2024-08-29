using System.Collections.Generic;
using UnityEngine;

public class TextureTransition : MonoBehaviour
{
    public Material transitionMaterial;
    public List<Texture> textures;
    public float transitionSpeed = 1f;
    [Range(0, 1)]
    public float noiseIntensity = 1f;
    [Range(0.01f, 1f)]
    public float smoothness = 0.5f;

    private float transition = 0f;
    private int currentTextureIndex = 0;
    private int nextTextureIndex = 1;

    void Start()
    {
        /*if (textures.Count < 2)
        {
            Debug.LogError("You need at least two textures to perform a transition.");
            enabled = false;
            return;
        }*/

        // Initialize the first two textures
        /*transitionMaterial.SetTexture("_CurrentTex", textures[currentTextureIndex]);
        transitionMaterial.SetTexture("_NextTex", textures[nextTextureIndex]);*/
    }

    void Update()
    {
        if (textures.Count < 2)
        {
            return;
        }

        // Animate the transition value
        transition += Time.deltaTime * transitionSpeed;
        if (transition > 1.0f)
        {
            transition = 0f;
            currentTextureIndex = nextTextureIndex;
            nextTextureIndex = (nextTextureIndex + 1) % textures.Count;

            // Update the textures in the shader
            transitionMaterial.SetTexture("_CurrentTex", textures[currentTextureIndex]);
            transitionMaterial.SetTexture("_NextTex", textures[nextTextureIndex]);
        }

        transitionMaterial.SetFloat("_Transition", transition);
        transitionMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        transitionMaterial.SetFloat("_Smoothness", smoothness);
    }
}
