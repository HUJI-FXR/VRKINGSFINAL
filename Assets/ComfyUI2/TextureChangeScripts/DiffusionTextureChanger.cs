using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class DiffusionTextureChanger : MonoBehaviour
{
    public float changeTextureEvery;
    public bool changeTextureToChildren;

    protected List<Texture2D> diff_Textures = new List<Texture2D>();
    protected int curTextureIndex = 0;

    public void AddTexture(Texture2D texture, bool addToTextureTotal)
    {
        if (texture == null)
        {
            return;
        }
        if (!addToTextureTotal)
        {
            curTextureIndex = 0;
            diff_Textures.Clear();
        }
        diff_Textures.Add(texture);
    }

    public void AddTexture(List<Texture2D> diffTextures, bool addToTextureTotal)
    {
        if (diffTextures == null)
        {
            return;
        }

        if (!addToTextureTotal)
        {
            curTextureIndex = 0;
            diff_Textures.Clear();
        }
        foreach (Texture2D texture in diffTextures)
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
        renderer.material.SetTexture("_MainTex", diff_Textures[curTextureIndex]);
    }
}
