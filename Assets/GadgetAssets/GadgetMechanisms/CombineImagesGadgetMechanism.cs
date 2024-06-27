using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;



public class CombineImagesGadgetMechanism : GadgetMechanism
{
    // TODO notice this CAN be generalized to larger number of objects, but requires many changes.
    public Queue<GameObject> selectedObjects = new Queue<GameObject>();
    public int MAX_QUEUED_OBJECTS = 2;

    public DiffusionRequest diffusionRequest;

    /*
    diffusionRequest.positivePrompt = "Beautiful";
    diffusionRequest.negativePrompt = "watermark";
    diffusionRequest.numOfVariations = 5;
    diffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
    diffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;
    diffusionRequest.diffusionModel = diffusionModels.ghostmix;*/
    /*public CombineImagesGadgetMechanism()
    {        
        mechanismText = MECHANISM_PRETEXT + "Combine Two Images";
        buttonText = "Combine";

        diffusionRequest = new DiffusionRequest();
        diffusionRequest.positivePrompt = "Beautiful";
        diffusionRequest.negativePrompt = "watermark";
        diffusionRequest.numOfVariations = 5;
        diffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        diffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;
        diffusionRequest.diffusionModel = diffusionModels.ghostmix;
    }*/

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
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

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
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

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
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

    /*public override void OnClick()
    {
        GetTexturesFromSelected();
    }*/

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

        diffusionRequest.uploadImage = copyTexture;
        diffusionRequest.secondUploadImage = secondCopyTexture;

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public override void PlaceTextureInput(InputAction.CallbackContext context)
    {
        Texture2D curTexture = GameManager.getInstance().gadget.getGeneratedTexture();
        if (curTexture == null)
        {
            Debug.LogError("Tried to add a textures from the Gadget camera without textures in the Queue");
            return;
        }

        // Perform the raycast
        Ray ray = new Ray(GameManager.getInstance().gadget.transform.position, GameManager.getInstance().gadget.transform.forward);
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
    public override void ActivateGeneration(InputAction.CallbackContext context)
    {
        GetTexturesFromSelected();
    }
}