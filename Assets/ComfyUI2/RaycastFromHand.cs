using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class RaycastFromHand : MonoBehaviour
{
    // TODO make Diffusables and Organizer a disconnected global variable
    public GameObject Diffusables;
    public DiffusionRequest DiffReq;
    public ComfyOrganizer Organizer;

    // TODO generalize this, instead of making specific variables for each mechanism, make it work for all mechanisms at once with an enum for choosing between the mechanics.

    private Queue<GameObject> selectedObjects = new Queue<GameObject>();
    private int MAX_QUEUED_OBJECTS = 2;

    

    private Color preSelectColor = new Color(0, 0, 255);
    private Color selectColor = new Color(0, 255, 0);
    private float outlineWidth = 20;

    //TODO DELETE BELOW
    public GameObject go1;
    public GameObject go2;

    private void Start()
    {
        Texture go1Text = go1.GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = go1.GetComponent<Renderer>().material.mainTexture;

        Texture2D copyTexture = toTexture2D(go1Text);
        Texture2D secondCopyTexture = toTexture2D(go2Text);

        DiffReq.uploadImage = copyTexture;
        DiffReq.secondUploadImage = secondCopyTexture;

        Organizer.SendDiffusionRequest(DiffReq);
    }

    // TODO remove all Diffusables == null statements after finishing with diffusables placement
    public void OnUIHoverEntered(UIHoverEventArgs args)
    {
        if (args == null || args.uiObject == null) {
            return;
        }

        if (args.uiObject.TryGetComponent<UnityEngine.UI.Image>(out UnityEngine.UI.Image innerImg))
        {
            GetComponent<Image>().sprite = innerImg.sprite;          
        }
    }

    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (Diffusables == null)
        {
            return;
        }
        
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != Diffusables.transform)
        {
            return;
        }

        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        // Creates pre-selection outline
        AddOutline(args.interactableObject.transform.gameObject, preSelectColor, outlineWidth);
    }

    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (Diffusables == null)
        {
            return;
        }

        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != Diffusables.transform)
        {
            
            return;
        }

        // Remove pre-selection outline
        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        DeleteOutline(args.interactableObject.transform.gameObject);
    }

    public void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (Diffusables == null)
        {
            return;
        }
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != Diffusables.transform)
        {
            return;
        }

        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        // Adds to queue of selected objects
        if (selectedObjects.Count >= MAX_QUEUED_OBJECTS)
        {
            GameObject dequeObject = selectedObjects.Dequeue();
            DeleteOutline(dequeObject);
        }

        selectedObjects.Enqueue(args.interactableObject.transform.gameObject);
        // Creates selection outline
        AddOutline(args.interactableObject.transform.gameObject, selectColor, outlineWidth);

        GetTexturesFromSelected();
    }

    private void DeleteOutline(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }
        if (obj.TryGetComponent<Outline>(out Outline curOutline))
        {
            Destroy(curOutline);
        }
    }
    private void AddOutline(GameObject obj, Color outlineColor, float outlineWidth)
    {
        if (obj == null)
        {
            return;
        }
        if (obj.TryGetComponent<Outline>(out Outline curOutline))
        {
            curOutline.OutlineColor = outlineColor;
            curOutline.OutlineWidth = outlineWidth;
            return;
        }
        Outline elseOutline = obj.AddComponent<Outline>();
        elseOutline.OutlineColor = outlineColor;
        elseOutline.OutlineWidth = outlineWidth;
    }

    public void GetTexturesFromSelected()
    {
        if (selectedObjects.Count != MAX_QUEUED_OBJECTS)
        {
            return;
        }

        Texture firstMap = selectedObjects.Dequeue().GetComponent<Renderer>().material.GetTexture("_MainTex");
        RenderTexture firstRt = new RenderTexture(firstMap.width, firstMap.height, 3, RenderTextureFormat.Default);
        Graphics.Blit(firstMap, firstRt);
        DiffReq.uploadImage = toTexture2D(firstRt);


        RenderTexture secondRt = new RenderTexture(firstMap.width, firstMap.height, 3, RenderTextureFormat.Default);
        Graphics.Blit(firstMap, secondRt);
        DiffReq.secondUploadImage = toTexture2D(secondRt);

        Organizer.SendDiffusionRequest(DiffReq);
    }

    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    Texture2D toTexture2D(Texture inTex)
    {
        RenderTexture rTex = new RenderTexture(inTex.width, inTex.height, 4);
        Graphics.Blit(inTex, rTex);

        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();

        return tex;
    }
}
