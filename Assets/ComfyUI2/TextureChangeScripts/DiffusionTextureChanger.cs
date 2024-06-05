using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.UI;
using UnityEngine;

public class DiffusionTextureChanger : MonoBehaviour
{
    protected List<Texture2D> diff_Textures = new List<Texture2D>();
    protected int curTextureIndex = 0;

    public virtual void AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        if (newDiffTextures == null)
        {
            return;
        }

        if (!addToTextureTotal)
        {
            curTextureIndex = 0;
            diff_Textures = new List<Texture2D>();
        }

        foreach (Texture2D texture in newDiffTextures)
        {
            diff_Textures.Add(texture);
        }
    }

    protected void changeTextureOn(GameObject curGameObject, Texture2D texture)
    {
        if (curGameObject == null || texture == null)
        {
            return;
        }
        Renderer renderer = curGameObject.GetComponent<Renderer>();
        renderer.material.SetTexture("_BaseMap", diff_Textures[curTextureIndex]);
    }
}
