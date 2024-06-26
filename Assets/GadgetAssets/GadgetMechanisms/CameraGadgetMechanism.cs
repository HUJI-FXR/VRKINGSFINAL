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

    /*
    diffusionRequest.positivePrompt = "Beautiful scene";
    diffusionRequest.negativePrompt = "watermark";
    diffusionRequest.numOfVariations = 5;
    diffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
    diffusionRequest.diffusionModel = diffusionModels.ghostmix;
    diffusionRequest.denoise = 0.4f;*/

    private bool TookImage = false;
    public bool UseStyleTransfer = true;

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public override void TakeTextureInput(InputAction.CallbackContext context)
    {
        if (TookImage || diffusionRequest.secondUploadImage == null)
        {
            return;
        }
        if (UseStyleTransfer)
        {
            diffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;
            TookImage = true;
        }

        // TODO add DiffusableObject data entry for diffusionrequest when taking a picture of stuff
        GameManager.getInstance().gadget.screenRecorder.CaptureScreenshot(diffusionRequest);
        GameManager.getInstance().gadget.gadgetCamera.enabled = false;
        GameManager.getInstance().gadget.xrCamera.enabled = true;
    }



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

                if (UseStyleTransfer)
                {
                    TookImage = false;
                    diffusionRequest.uploadImage = null;
                    diffusionRequest.secondUploadImage = null;
                }
            }
        }
    }

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (!UseStyleTransfer || TookImage)
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
        // Creates pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!UseStyleTransfer || TookImage)
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
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (!UseStyleTransfer || TookImage)
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
        // Creates selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
        Texture2D curTexture = TextureManipulationLibrary.toTexture2D(args.interactableObject.transform.gameObject.GetComponent<Renderer>().material.mainTexture);
        diffusionRequest.secondUploadImage = curTexture;
    }
}
