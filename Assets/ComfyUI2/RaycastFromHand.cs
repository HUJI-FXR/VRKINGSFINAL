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
    public DiffusionRequest diffRequest;

    // TODO generalize this, instead of making specific variables for each mechanism, make it work for all mechanisms at once with an enum for choosing between the mechanics.

    private Queue<GameObject> selectedObjects = new Queue<GameObject>();
    private int MAX_QUEUED_OBJECTS = 2;

    

    private Color preSelectColor = new Color(0, 0, 255);
    private Color selectColor = new Color(0, 255, 0);
    private float outlineWidth = 20;

    //TODO DELETE BELOW
    public GameObject go1;
    public GameObject go2;

    /*private void Start()
    {
        Texture go1Text = go1.GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = go2.GetComponent<Renderer>().material.mainTexture;

        Texture2D copyTexture = GeneralGameScript.instance.comfySceneLibrary.toTexture2D(go1Text);
        Texture2D secondCopyTexture = GeneralGameScript.instance.comfySceneLibrary.toTexture2D(go2Text);

        copyTexture.name = GeneralGameScript.instance.comfyOrganizer.UniqueImageName() + ".png";
        secondCopyTexture.name = GeneralGameScript.instance.comfyOrganizer.UniqueImageName() + "_2" + ".png";

        Debug.Log(secondCopyTexture.name);

        diffRequest.uploadImage = copyTexture;
        diffRequest.secondUploadImage = secondCopyTexture;

        GeneralGameScript.instance.comfyOrganizer.SendDiffusionRequest(diffRequest);
    }*/

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
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GeneralGameScript.instance.diffusables.transform)
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
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GeneralGameScript.instance.diffusables.transform)
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
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GeneralGameScript.instance.diffusables.transform)
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

        /*Texture go1Text = selectedObjects.Dequeue().GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = selectedObjects.Dequeue().GetComponent<Renderer>().material.mainTexture;

        Texture2D copyTexture = GeneralGameScript.instance.comfySceneLibrary.toTexture2D(go1Text);
        Texture2D secondCopyTexture = GeneralGameScript.instance.comfySceneLibrary.toTexture2D(go2Text);

        diffRequest.uploadImage = copyTexture;
        diffRequest.secondUploadImage = secondCopyTexture;

        GeneralGameScript.instance.comfyOrganizer.SendDiffusionRequest(diffRequest);*/



        /*Texture go1Text = go1.GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = go2.GetComponent<Renderer>().material.mainTexture;*/
        Texture go1Text = selectedObjects.Dequeue().GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = selectedObjects.Dequeue().GetComponent<Renderer>().material.mainTexture;

        Texture2D copyTexture = GeneralGameScript.instance.comfySceneLibrary.toTexture2D(go1Text);
        Texture2D secondCopyTexture = GeneralGameScript.instance.comfySceneLibrary.toTexture2D(go2Text);

        copyTexture.name = GeneralGameScript.instance.comfyOrganizer.UniqueImageName() + ".png";
        secondCopyTexture.name = GeneralGameScript.instance.comfyOrganizer.UniqueImageName() + "_2" + ".png";

        Debug.Log(secondCopyTexture.name);

        diffRequest.uploadImage = copyTexture;
        diffRequest.secondUploadImage = secondCopyTexture;

        GeneralGameScript.instance.comfyOrganizer.SendDiffusionRequest(diffRequest);

        /*Texture firstMap = selectedObjects.Dequeue().GetComponent<Renderer>().material.GetTexture("_MainTex");
        RenderTexture firstRt = new RenderTexture(firstMap.width, firstMap.height, 3, RenderTextureFormat.Default);
        Graphics.Blit(firstMap, firstRt);
        diffRequest.uploadImage = toTexture2D(firstRt);


        RenderTexture secondRt = new RenderTexture(firstMap.width, firstMap.height, 3, RenderTextureFormat.Default);
        Graphics.Blit(firstMap, secondRt);
        diffRequest.secondUploadImage = toTexture2D(secondRt);

        GeneralGameScript.comfyOrganizer.SendDiffusionRequest(diffRequest);*/
    }


}
