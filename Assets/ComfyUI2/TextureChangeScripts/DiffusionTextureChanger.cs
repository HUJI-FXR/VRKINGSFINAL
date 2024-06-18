using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.UI;
using UnityEngine;

public class DiffusionTextureChanger : MonoBehaviour
{
    protected List<Texture2D> diff_Textures = new List<Texture2D>();
    protected int curTextureIndex = 0;

    public virtual bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (diffusionRequest.textures == null)
        {
            return false;
        }

        if (!diffusionRequest.addToTextureTotal)
        {
            curTextureIndex = 0;
            diff_Textures = new List<Texture2D>();
            diff_Textures.Clear();
        }

        foreach (Texture2D texture in diffusionRequest.textures)
        {
            diff_Textures.Add(texture);
        }

        return true;
    }

    // TODO older script of doing this, decided to make the diffusionrequest go all the way through to the end
    public virtual bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        if (newDiffTextures == null)
        {
            return false;
        }

        if (!addToTextureTotal)
        {
            curTextureIndex = 0;
            diff_Textures = new List<Texture2D>();
            diff_Textures.Clear();
        }

        foreach (Texture2D texture in newDiffTextures)
        {
            diff_Textures.Add(texture);
        }

        return true;
    }

    protected virtual void changeTextureOn(GameObject curGameObject, Texture2D texture)
    {
        if (curGameObject == null || texture == null)
        {
            return;
        }
        Renderer renderer = curGameObject.GetComponent<Renderer>();
        renderer.material.SetTexture("_BaseMap", diff_Textures[curTextureIndex]);
    }
}
