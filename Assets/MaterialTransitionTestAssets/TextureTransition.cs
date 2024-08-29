using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used on GameObject in the world for which we want to transition between textures in a smooth way.
/// Used in conjunction with the TextureTransitionShader.
/// </summary>
public class TextureTransition : MonoBehaviour
{
    public Material transitionMaterial;

    // Textures to be cycled through
    public List<Texture> textures;

    public float transitionSpeed = 1f;
    [Range(0, 1)]
    public float noiseIntensity = 1f;
    [Range(0.01f, 1f)]
    public float smoothness = 0.5f;

    private float transition = 0f;

    // Counter for the textures that are being cycled through
    private int currentTextureIndex = 0;
    private int nextTextureIndex = 1;


    void Update()
    {

        // If there are less than two textures, there is no cycling through them
        if (textures.Count < 2)
        {
            // If there is a single texture, it is the only seen texture
            if (textures.Count == 1)
            {
                transitionMaterial.SetTexture("_CurrentTex", textures[0]);
            }
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
