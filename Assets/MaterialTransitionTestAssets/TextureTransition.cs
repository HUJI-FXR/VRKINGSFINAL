using System;
using System.Collections.Generic;
using UnityEngine;


// TODO end of transition is too rapid, need smoothness when exchanging 2 textures


/// <summary>
/// Used on GameObject in the world for which we want to transition between textures in a smooth way.
/// Used in conjunction with the TextureTransitionShader.
/// </summary>
public class TextureTransition : MonoBehaviour
{
    // Textures to be cycled through
    public List<Texture> textures;

    // Simple optimization to stop retexturing a single texture
    private bool singleTexture = false;

    // True when Textures are endlessly and automatically transitioned from one to another.
    // False when one single transition, from texture to texture, will occur, and then stop instead of continuing onto the next transition.
    public bool constantTransition = true;

    public float transitionSpeed = 1f;
    [Range(0, 1)]
    public float noiseIntensity = 1f;
    private float m_noiseIntensity = 1f;
    [Range(0.01f, 1f)]
    public float smoothness = 0.5f;
    private float m_smoothness = 0.5f;

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
        if (textures == null || transitionMaterial == null) return;
        if (textures.Count <= 0) return;

        if (m_noiseIntensity != noiseIntensity)
        {
            m_noiseIntensity = noiseIntensity;
            transitionMaterial.SetFloat("_NoiseIntensity", noiseIntensity);
        }
        if (m_smoothness != smoothness)
        {
            m_smoothness = smoothness;
            transitionMaterial.SetFloat("_Smoothness", smoothness);
        }

        if (textures.Count == 1)
        {
            if (singleTexture) return;
            transitionMaterial.SetTexture("_NextTex", textures[0]);
            transitionMaterial.SetFloat("_Transition", 1f);
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
    }

    /// <summary>
    /// Triggers the next transition in the list
    /// </summary>
    public void TriggerNextTexture()
    {
        if (textures == null || transitionMaterial == null) return;
        if (textures.Count <= 0) return;

        transition = 0f;
        currentTextureIndex = nextTextureIndex;
        nextTextureIndex = (nextTextureIndex + 1) % textures.Count;

        // Update the textures in the shader
        transitionMaterial.SetTexture("_CurrentTex", textures[currentTextureIndex]);
        transitionMaterial.SetTexture("_NextTex", textures[nextTextureIndex]);
    }

    public void ResetTransition()
    {
        if (transitionMaterial == null) return;

        textures = new List<Texture>();
        transition = 0;
        currentTextureIndex = 0;
        nextTextureIndex = 1;

        transitionMaterial.SetTexture("_CurrentTex", null);
        transitionMaterial.SetTexture("_NextTex", null);
    }

    /// <summary>
    /// Changes all the relevant parameters in the Texture Transition and restarts it with these parameters.
    /// </summary>
    /// <param name="curTextures">New Textures of the TransitionTexture</param>
    /// <param name="curTransition">Transition value, if out of range, will not change the current value</param>
    /// <param name="curNoiseIntensity">Noise Intensity value, if out of range, will not change the current value</param>
    /// <param name="curSmoothness">Smoothness value, if out of range, will not change the current value</param>
    public void TransitionTextures(List<Texture> curTextures, float curTransition, float curNoiseIntensity, float curSmoothness)
    {
        if (curTransition >= 0 && curTransition <= 1)
        {
            transition = curTransition;
        }
        if (curNoiseIntensity >= 0.01f && curNoiseIntensity <= 1)
        {
            noiseIntensity = curNoiseIntensity;
        }
        if (curSmoothness >= 0 && curSmoothness <= 1)
        {
            smoothness = curSmoothness;
        }

        ResetTransition();

        textures = curTextures;
        
        TriggerNextTexture();
    }
}
