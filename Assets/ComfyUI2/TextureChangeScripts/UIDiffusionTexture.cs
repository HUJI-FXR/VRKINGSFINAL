using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIDiffusionTexture : DiffusionTextureChanger
{
    public GameObject UIDisplay;
    public GameObject displayPrefab;

    private GameObject curDisplayPrefab;

    private bool displayTextures = false;
    private float changeRate = 3.0f;
    private float curChangeDelta = 0f;

    private static float IMAGES_REDUCE_SIZE_FACTOR = 1;
    public override bool AddTexture(List<Texture2D> textures, bool addToTextureTotal)
    {
        if (UIDisplay == null || displayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return false;
        }
        if (base.AddTexture(textures, addToTextureTotal))
        {
            curChangeDelta = 0f;

            foreach (Transform child in UIDisplay.transform)
            {
                DestroyImmediate(child.gameObject);          
            }
            displayTextures = false;

            if (curDisplayPrefab != null)
            {
                Destroy(curDisplayPrefab);
                curDisplayPrefab = null;
            }
            curDisplayPrefab = Instantiate(displayPrefab, UIDisplay.transform, false);

            foreach (Texture2D tex in diff_Textures)
            {
                GameObject childGameObject = new GameObject("Image");

                // Set the new GameObject as a child of the parentGameObject
                childGameObject.transform.SetParent(curDisplayPrefab.transform, false);

                // Add a RectTransform component to the child GameObject if not already present
                RectTransform rectTransform = childGameObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(tex.width / IMAGES_REDUCE_SIZE_FACTOR, tex.height / IMAGES_REDUCE_SIZE_FACTOR); // Adjust the size as needed

                // Add an Image component to the child GameObject
                childGameObject.AddComponent<Image>();
            }

            for (int i = 0; i < diff_Textures.Count; i++)
            {
                GameObject go = curDisplayPrefab.transform.GetChild(i).gameObject;
                if (go != null)
                {
                    changeTextureOn(go, diff_Textures[i]);
                }
            }

            displayTextures = true;
            return true;
        }

        return false;
    }

    private float totalChangeDelta(float curDelta, float totalDelta)
    {
        // TODO can do this with a min statement between these two if options
        if (curDelta >= totalDelta/2)
        {
            return (totalDelta - curDelta) / (totalDelta/2);
        }
        return curDelta / (totalDelta/2);
    }

    private void Update()
    {
        if (displayTextures || curDisplayPrefab != null)
        {
            curChangeDelta += Time.deltaTime;

            float curTotalChangeDelta = totalChangeDelta(curChangeDelta, changeRate);
            // Assume all children of UIDisplay are Images
            foreach (Transform child in curDisplayPrefab.transform)
            {
                Image curImage = child.GetComponent<Image>();
                // 1 - (curChangeDelta / changeRate)
                curImage.color = new Color(curImage.color.r, curImage.color.g, curImage.color.b, curTotalChangeDelta);
            }

            Image displayImage = curDisplayPrefab.GetComponent<Image>();
            // 1 - (curChangeDelta / changeRate)
            displayImage.color = new Color(displayImage.color.r, displayImage.color.g, displayImage.color.b, curTotalChangeDelta);

            if (curChangeDelta > changeRate)
            {
                displayTextures = false;                
            }
        }
        else
        {
            curChangeDelta = 0f;
        }        
    }

    protected override void changeTextureOn(GameObject curGameObject, Texture2D texture)
    {
        if (curGameObject == null || texture == null)
        {
            Debug.LogError("Tried to change texture while the texture or target GameObject doesn't exist");
            return;
        }

        Sprite curSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        Image curImage = curGameObject.GetComponent<Image>();
        curImage.sprite = curSprite;
    }
}
