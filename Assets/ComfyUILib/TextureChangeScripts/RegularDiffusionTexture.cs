using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.Events;

public class RegularDiffusionTexture : DiffusionTextureChanger
{
    public float changeTextureEvery = 1;
    public bool changeTextureToChildren = false;
    public UnityEvent AddedTextureUnityEvent;

    private float textureChangeDelta = 0;

    // Update is called once per frame
    protected void Update()
    {
        // TODO should be more than 1, because otherwise only one change is necessary and should be down in addimage function
        if (diff_Textures.Count > 0)
        {
            textureChangeDelta += Time.deltaTime;
            if (textureChangeDelta > changeTextureEvery)
            {
                if (changeTextureToChildren)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GameObject child = transform.GetChild(i).gameObject;
                        base.changeTextureOn(child, diff_Textures[curTextureIndex]);
                    }
                }
                else
                {
                    Debug.Log("2COUNT " + diff_Textures.Count);
                    Debug.Log("2COUNTIND " + curTextureIndex);
                    Debug.Log(diff_Textures[curTextureIndex] == null);
                    base.changeTextureOn(gameObject, diff_Textures[curTextureIndex]);
                }

                curTextureIndex++;
                curTextureIndex %= diff_Textures.Count;

                textureChangeDelta = 0;
            }
        }
    }



    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        /*foreach (Texture2D tex in diffusionRequest.textures)
        {
            Debug.Log("NAME " +  tex.name);
        }*/

        bool ret = base.AddTexture(diffusionRequest);
        if (ret) AddedTextureUnityEvent?.Invoke();
        return ret;
    }

    public override bool AddTexture(List<Texture2D> newDiffTextures, bool addToTextureTotal)
    {
        bool ret = base.AddTexture(newDiffTextures, addToTextureTotal);
        if (ret) AddedTextureUnityEvent?.Invoke();
        return ret;
    }

}
