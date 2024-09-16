using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIDiffusionTexture : DiffusionTextureChanger
{
    public GameObject PopupDisplay;
    public GameObject imagesDisplayPrefab;    
    [SerializeField] public GameObject AIDisplayPrefab;    

    private GameObject curDisplayPrefab;

    private bool displayTextures = false;

    [Min(0.01f)]
    private float changeRate = 3.0f;

    private float curChangeDelta = 0f;

    public UnityEvent unityEvent;

    //private static float IMAGES_REDUCE_SIZE_FACTOR = 512;

    public PlayGadgetSounds playGadgetSounds;

    private void Start()
    {
        if (PopupDisplay == null || imagesDisplayPrefab == null)
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
            // rectTransform.localScale = new Vector2(IMAGES_REDUCE_SIZE_FACTOR / ((float)tex.width), IMAGES_REDUCE_SIZE_FACTOR / ((float)tex.height));
            //rectTransform.sizeDelta = new Vector2(tex.width / IMAGES_REDUCE_SIZE_FACTOR, tex.height / IMAGES_REDUCE_SIZE_FACTOR); // Adjust the size as needed
            
            
            rectTransform.sizeDelta = new Vector2(tex.width / 100, tex.height / 100);

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

    private void CreatePopupTemplate(List<Texture2D> textures, GameObject givenDispayPrefab)
    {
        if (textures.Count == 0)
        {
            Debug.Log("Tried creating popup with no textures");
            return;
        }
        
        if (PopupDisplay == null || imagesDisplayPrefab == null)
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
        
        
        curDisplayPrefab = Instantiate(givenDispayPrefab, PopupDisplay.transform, false);
        Debug.Log("Instantiated popup!");

        CreateImagesInside(textures, curDisplayPrefab, true);
        
        Debug.Log("Created images inside popup!");

        displayTextures = true;
        playGadgetSounds.PlaySound("ShowUIElement");
    }

    public void CreatePopup(List<Texture2D> textures)
    {
        CreatePopupTemplate(textures, imagesDisplayPrefab);
    }
    
    public void CreateAIPopup(List<Texture2D> textures)
    {
        CreatePopupTemplate(textures, AIDisplayPrefab);
    }

    public override bool AddTexture(DiffusionRequest diffusionRequest)
    {
        if (GameManager.getInstance().gadget == null) return false;

        if (PopupDisplay == null || imagesDisplayPrefab == null)
        {
            Debug.LogError("Add UI Display and Prefab for the Image UI popup");
            return false;
        }
        if (base.AddTexture(diffusionRequest))
        {
            CreatePopup(diff_Textures);
            
            GameManager.getInstance().gadget.AddTexturesToQueue(diff_Textures);

            // Sending broadcast to Game timeline script
            unityEvent?.Invoke();

            return true;
        }

        return false;
    }

    private void Update()
    {
        if (displayTextures && curDisplayPrefab != null)
        {
            curChangeDelta += Time.deltaTime;

            // Notice changeRate > 0
            float curChange = 2 * curChangeDelta / changeRate;
            float curTotalChangeDelta = Mathf.Min(curChange, 2 - curChange);

            // Assume all children of UIDisplay are Images
            foreach (Transform child in curDisplayPrefab.transform)
            {
                Debug.Log(curTotalChangeDelta);
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

                DestroyImmediate(curDisplayPrefab);
                curDisplayPrefab = null;
            }
        }
        else
        {
            curChangeDelta = 0f;
        }        
    }

    public override void changeTextureOn(GameObject curGameObject, Texture2D texture)
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
