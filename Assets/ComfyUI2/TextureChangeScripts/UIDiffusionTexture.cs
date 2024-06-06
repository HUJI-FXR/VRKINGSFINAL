using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIDiffusionTexture : DiffusionTextureChanger
{

    public Canvas UIDisplay;

    private bool displayTextures;

    private float changeRate = 3.0f;
    private float curChangeDelta = 0f;

    private List<GameObject> imagesGameObjects = new List<GameObject>();
    public override bool AddTexture(List<Texture2D> textures, bool addToTextureTotal)
    {
        if (UIDisplay == null)
        {
            return false;
        }
        if (base.AddTexture(textures, addToTextureTotal))
        {
            curChangeDelta = 0f;

            // Assume all children of UIDisplay are Images
            imagesGameObjects.Clear();
            for (int i = 0; i < UIDisplay.transform.childCount; i++)
            {
                GameObject curGameObject = UIDisplay.transform.GetChild(i).gameObject;


                imagesGameObjects.Add(curGameObject);

                //TODO understand how to change number of images per number of textures??
                changeTextureOn(curGameObject, textures[0]);
            }

            displayTextures = true;
            return true;
        }

        return false;
    }

    private void Update()
    {
        if (displayTextures)
        {
            curChangeDelta += Time.deltaTime;

            foreach (GameObject img in imagesGameObjects)
            {
                Image curImage = img.GetComponent<Image>();
                curImage.color = new Color(curImage.color.r, curImage.color.g, curImage.color.b, 1 - (curChangeDelta / changeRate));
            }

            if (curChangeDelta > changeRate)
            {
                curChangeDelta = 0f;
                displayTextures = false;
            }
        }
    }

    protected override void changeTextureOn(GameObject curGameObject, Texture2D texture)
    {
        if (curGameObject == null || texture == null)
        {
            return;
        }

        Sprite curSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        Image curImage = curGameObject.GetComponent<Image>();
        curImage.sprite = curSprite;
    }
}
