using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIDiffusionTexture : DiffusionTextureChanger
{
    public GameObject PopupDisplay;
    public GameObject displayPrefab;    

    private GameObject curDisplayPrefab;

    private bool displayTextures = false;
    private float changeRate = 3.0f;
    private float curChangeDelta = 0f;

    private static float IMAGES_REDUCE_SIZE_FACTOR = 512;

    public PlayGadgetSounds playGadgetSounds;

    private void Start()
    {
        if (PopupDisplay == null || displayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return;
        }

        if (playGadgetSounds == null)
        {
            Debug.LogError("Add all UIDiffusionTexture inputs");
        }
    }


    // Adding the Image in the Gadget panel as well
    // TODO should this part be in a separate place? should these textures have a global variable for global access? shouldn't the gadget deal with it? is UI and gadget sepearate?
    // TODO change with this and make a PopupDiffusionTexture instead?
    public void CreateImagesInside(List<Texture2D> textures, GameObject toBeParent, bool destroyPreviousChildren)
    {
        if (toBeParent == null)
        {
            return;
        }
        if (destroyPreviousChildren)
        {
            var children = new List<GameObject>();
            foreach (Transform child in toBeParent.transform) children.Add(child.gameObject);
            children.ForEach(child => DestroyImmediate(child));
        }
        if (textures == null || textures.Count == 0) {
            return;
        }

        foreach (Texture2D tex in textures)
        {
            GameObject childGameObject = new GameObject("Image");

            // Set the new GameObject as a child of the parentGameObject
            childGameObject.transform.SetParent(toBeParent.transform, false);

            // Add a RectTransform component to the child GameObject if not already present            
            RectTransform rectTransform = childGameObject.AddComponent<RectTransform>();            
            rectTransform.localScale = new Vector2(((float)tex.width) / IMAGES_REDUCE_SIZE_FACTOR, ((float)tex.height) / IMAGES_REDUCE_SIZE_FACTOR);
            //rectTransform.sizeDelta = new Vector2(tex.width / IMAGES_REDUCE_SIZE_FACTOR, tex.height / IMAGES_REDUCE_SIZE_FACTOR); // Adjust the size as needed

            // Add an Image component to the child GameObject
            Image curImage = childGameObject.AddComponent<Image>();
        }

        for (int i = 0; i < textures.Count; i++)
        {
            GameObject go = toBeParent.transform.GetChild(i).gameObject;
            if (go != null)
            {
                changeTextureOn(go, textures[i]);
            }
        }
    }    

    public void CreatePopup(List<Texture2D> textures)
    {
        if (PopupDisplay == null || displayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return;
        }

        curChangeDelta = 0f;

        if (curDisplayPrefab != null)
        {
            Destroy(curDisplayPrefab);
            curDisplayPrefab = null;
        }
        curDisplayPrefab = Instantiate(displayPrefab, PopupDisplay.transform, false);

        CreateImagesInside(textures, curDisplayPrefab, true);

        displayTextures = true;
        playGadgetSounds.PlaySound("ShowUIElement");
    }

    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (PopupDisplay == null || displayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return false;
        }
        if (base.AddTexture(diffusionRequest))
        {
            CreatePopup(diff_Textures);
            GameManager.getInstance().gadget.AddTexturesToQueue(diff_Textures);

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
        if (displayTextures && curDisplayPrefab != null)
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
