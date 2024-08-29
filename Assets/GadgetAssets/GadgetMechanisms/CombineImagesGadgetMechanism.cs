using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;


// TODO make a unique ID for everything, downloaded images, uploaded images, request IDs etc
public class CombineImagesGadgetMechanism : GadgetMechanism
{
    // TODO notice this CAN be generalized to larger number of objects, but requires many changes.
    public Queue<GameObject> selectedObjects = new Queue<GameObject>();
    public int MAX_QUEUED_OBJECTS = 2;

    private void Start()
    {
        mechanismText = "Combine Images";
    }

    // TODO we've changed all the mechanisms to be symmetric by hands, these separations are no longer needed
    public override void OnGameObjectLeftHoverEntered(HoverEnterEventArgs args)
    {
        OnGameObjectHoverEntered(args);
    }
    public override void OnGameObjectLeftHoverExited(HoverExitEventArgs args)
    {
        OnGameObjectHoverExited(args);
    }
    public override void OnGameObjectRightHoverEntered(HoverEnterEventArgs args)
    {
        OnGameObjectHoverEntered(args);
    }
    public override void OnGameObjectRightHoverExited(HoverExitEventArgs args)
    {
        OnGameObjectHoverExited(args);
    }
    public override void onGameObjectLeftSelectEntered(SelectEnterEventArgs args)
    {
        onGameObjectSelectEntered(args);
    }
    public override void onGameObjectRightSelectEntered(SelectEnterEventArgs args)
    {
        onGameObjectSelectEntered(args);
    }

    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {
            return;
        }

        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        // Creates pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {

            return;
        }

        // Remove pre-selection outline
        if (selectedObjects.Contains(args.interactableObject.transform.gameObject))
        {
            return;
        }
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
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
            GameManager.getInstance().gadget.ChangeOutline(dequeObject, GadgetSelection.unSelected);
        }

        selectedObjects.Enqueue(args.interactableObject.transform.gameObject);
        // Creates selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Combine Images Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest()
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;

        return newDiffusionRequest;
    }

    public void GetTexturesFromSelected()
    {
        if (selectedObjects.Count != MAX_QUEUED_OBJECTS)
        {
            return;
        }

        GameObject firstGameObject = selectedObjects.Dequeue();
        GameObject secondGameObject = selectedObjects.Dequeue();
        Texture go1Text = firstGameObject.GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = secondGameObject.GetComponent<Renderer>().material.mainTexture;

        GameManager.getInstance().gadget.ChangeOutline(firstGameObject, GadgetSelection.unSelected);
        GameManager.getInstance().gadget.ChangeOutline(secondGameObject, GadgetSelection.unSelected);

        Texture2D copyTexture = TextureManipulationLibrary.toTexture2D(go1Text);
        Texture2D secondCopyTexture = TextureManipulationLibrary.toTexture2D(go2Text);

        string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
        copyTexture.name = uniqueName + ".png";
        secondCopyTexture.name = uniqueName + "_2" + ".png";

        DiffusionRequest diffusionRequest = CreateDiffusionRequest();

        diffusionRequest.uploadTextures.Add(copyTexture);
        diffusionRequest.uploadTextures.Add(secondCopyTexture);

        diffusionRequest.positivePrompt = "";
        if (firstGameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DiffObj))
        {
            diffusionRequest.positivePrompt += DiffObj.keyword;
        }
        if (secondGameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DiffObjSec))
        {
            diffusionRequest.positivePrompt += DiffObjSec.keyword;
        }

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public override void PlaceTextureInput(GameObject GO)
    {
        if (GO == null) return;

        Texture2D curTexture = GameManager.getInstance().gadget.getGeneratedTexture();
        if (curTexture == null)
        {
            Debug.Log("Tried to add a textures from the Gadget camera without textures in the Queue");
            return;
        }

        // Perform the raycast
        Ray ray = new Ray(GO.transform.position, GO.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit GameObject has the TextureScript component


            //TODO maybe this is a wrong choice to make ANOTHER diffusion request because this one isn't really a diffusion request at all, just a transfer from one
            // texturechange to another
            if (hit.collider.gameObject.TryGetComponent<DiffusionTextureChanger>(out DiffusionTextureChanger dtc))
            {
                dtc.AddTexture(new List<Texture2D>() { curTexture }, false);
            }
        }
    }
    public override void ActivateGeneration(GameObject GO)
    {
        GetTexturesFromSelected();
    }
}