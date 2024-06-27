using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;


public class CameraGadgetMechanism : GadgetMechanism
{
    // todo break into two parts, one the input the other the output through the comfy lib
    public DiffusionRequest diffusionRequest;
    public bool UseStyleTransfer = true;

    private GameObject selectedStyleObject = null;

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    /*public override void TakeTextureInput(InputAction.CallbackContext context)
    {
        return;
    }*/



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

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (!UseStyleTransfer)
        {
            return;
        }

        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {
            return;
        }
        if (args.interactableObject.transform.gameObject == selectedStyleObject)
        {
            return;
        }

        // Creates pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!UseStyleTransfer)
        {
            return;
        }
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {

            return;
        }
        if (args.interactableObject.transform.gameObject == selectedStyleObject)
        {
            return;
        }

        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (!UseStyleTransfer)
        {
            return;
        }
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {
            return;
        }

        // Removing previously selected object
        if (selectedStyleObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }

        // Creates selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
        selectedStyleObject = args.interactableObject.transform.gameObject;
        Texture2D curTexture = TextureManipulationLibrary.toTexture2D(args.interactableObject.transform.gameObject.GetComponent<Renderer>().material.mainTexture);
        diffusionRequest.secondUploadImage = curTexture;
    }

    public override void ActivateGeneration(InputAction.CallbackContext context)
    {
        if (diffusionRequest.uploadImage == null || diffusionRequest.secondUploadImage == null)
        {
            Debug.Log("Need to select a style and take a screenshot");
            return;
        }

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        return;
    }

    public override void TakeScreenshot(InputAction.CallbackContext context)
    {
        // TODO add DiffusableObject data entry for diffusionrequest when taking a picture of stuff
        Texture2D screenShot = GameManager.getInstance().gadget.screenRecorder.CaptureScreenshot(diffusionRequest);
        GameManager.getInstance().gadget.gadgetCamera.enabled = false;
        GameManager.getInstance().gadget.xrCamera.enabled = true;

        diffusionRequest.uploadImage = screenShot;

        if (selectedStyleObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }

        if (!UseStyleTransfer)
        {
            diffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;
            GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        }
    }
}
