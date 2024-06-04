using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularDiffusionTexture : DiffusionTextureChanger
{
    private float textureChangeDelta = 0;

    // Update is called once per frame
    void Update()
    {
        if (diff_Textures.Count <= 0)
        {
            return;
        }

        textureChangeDelta += Time.deltaTime;
        if (textureChangeDelta > changeTextureEvery)
        {
            if (changeTextureToChildren)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject child = transform.GetChild(i).gameObject;
                    changeTextureOn(child, diff_Textures[curTextureIndex]);
                }
            }
            else
            {
                changeTextureOn(gameObject, diff_Textures[curTextureIndex]);
            }

            curTextureIndex++;
            curTextureIndex %= diff_Textures.Count;

            textureChangeDelta = 0;
        }
    }
}
