using System;
using System.Collections.Generic;
using Unity.Android.Types;
using UnityEngine;


/// <summary>
/// Used on GameObject in the world for which we want to transition between textures in a smooth way.
/// Used in conjunction with the TextureTransitionShader.
/// </summary>
public class TextureTransition : MonoBehaviour
{
    // Textures to be cycled through
    public List<Texture2D> textures;

    // Simple optimization to stop retexturing a single texture
    private bool singleTexture = false;

    // True when Textures are endlessly and automatically transitioned from one to another.
    // False when one single transition, from texture to texture, will occur, and then stop instead of continuing onto the next transition.
    public bool constantTransition = true;

    public float transitionSpeed = 1f;
    [Range(0, 1)]
    public float noiseIntensity = 1f;
    [Range(0.01f, 1f)]
    public float smoothness = 0.5f;

    [NonSerialized]
    public float transition = 0f;

    // Counter for the textures that are being cycled through
    private int currentTextureIndex = 0;
    private int nextTextureIndex = 1;

    private Material transitionMaterial = null;    

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;
        if (renderer.material == null) return;

        // Checks if the current shader is appropriate as per its properties
        if (!renderer.material.HasProperty("_CurrentTex") ||
            !renderer.material.HasProperty("_NextTex")    ||
            !renderer.material.HasProperty("_Transition") ||
            !renderer.material.HasProperty("_Smoothness") ||
            !renderer.material.HasProperty("_NoiseIntensity"))
        {
            Debug.LogError("Add correct shader to Game Object " + name);
            return;
        }

        transitionMaterial = renderer.material;
    }

    void Update()
    {
        if (textures == null) return;
        if (textures.Count <= 0) return;

        if (textures.Count == 1)
        {
            if (singleTexture) return;
            transitionMaterial.SetTexture("_CurrentTex", textures[0]);
            singleTexture = true;
            return;
        }        

        singleTexture = false;        

        // Animate the transition value
        transition += Time.deltaTime * transitionSpeed;
        if (transition > 1.0f)
        {
            // Stops the automatic step to the next transition in the list
            if (!constantTransition) return;

            TriggerNextTexture();
        }

        // Sends the needed parameters to the shader to look appropriate in accordance with the transition
        transitionMaterial.SetFloat("_Transition", transition);
        transitionMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        transitionMaterial.SetFloat("_Smoothness", smoothness);
    }

    /// <summary>
    /// Triggers the next transition in the list
    /// </summary>
    public void TriggerNextTexture()
    {
        if (textures == null) return;
        if (textures.Count <= 0) return;
        if (transition <= 1.0f) return;

        transition = 0f;
        currentTextureIndex = nextTextureIndex;
        nextTextureIndex = (nextTextureIndex + 1) % textures.Count;

        // Update the textures in the shader
        transitionMaterial.SetTexture("_CurrentTex", textures[currentTextureIndex]);
        transitionMaterial.SetTexture("_NextTex", textures[nextTextureIndex]);
    }
}
