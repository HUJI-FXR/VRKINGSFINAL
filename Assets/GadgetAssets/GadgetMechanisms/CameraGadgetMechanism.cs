using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;


// TODO write comments explaining everything for mechanisms
public class CameraGadgetMechanism : GadgetMechanism
{
    // todo break into two parts, one the input the other the output through the comfy lib
    public DiffusionRequest diffusionRequest;
    public bool UseStyleTransfer = true;

    // Object that was selected for the texture to be used as the style base for the Camera's image.
    private GameObject selectedStyleObject = null;

    private void Start()
    {
        mechanismText = "Base Camera";
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //

    /// <summary>
    /// Places a texture on the object that was selected with the controllers' Trigger button. 
    /// The Object is selected with a ray that comes from the left hand.
    /// </summary>
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

    
    // The Mechanism chooses the style of the Camera texture diffusion with the right hand
    public override void OnGameObjectRightHoverEntered(HoverEnterEventArgs args)
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
    public override void OnGameObjectRightHoverExited(HoverExitEventArgs args)
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
    public override void onGameObjectRightSelectEntered(SelectEnterEventArgs args)
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

        string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
        curTexture.name = uniqueName + "_2.png";

        // Style texture is the second one in the uploadTextures List,
        // if there is already one, replace it with the current one.
        if (diffusionRequest.uploadTextures.Count >= 2)
        {
            diffusionRequest.uploadTextures[1] = curTexture;
        }        
    }

    // The function is called with the right hand.
    /// <summary>
    /// Activates the Diffusion image generation using the right hand controller. 
    /// For the generation to begin, a camera shot and a style texture need to be picked to be sent to the generator.
    /// </summary>
    public override void ActivateGeneration(InputAction.CallbackContext context)
    {
        if (diffusionRequest.uploadTextures == null)
        {
            Debug.LogError("Need to add textures to the camera workflow");
            return;
        }
        if (diffusionRequest.uploadTextures.Count <= 1)
        {
            Debug.LogError("Need to add enough textures to the camera workflow");
            return;
        }

        if (selectedStyleObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        return;
    }

    // The function is called with the left hand, and the camera is also positioned on the left hand.
    /// <summary>
    /// Shoots an image with the Gadget left-hand camera.
    /// </summary>
    public override void TakeScreenshot(InputAction.CallbackContext context)
    {
        // TODO add DiffusableObject data entry for diffusionrequest when taking a picture of stuff
        Texture2D screenShot = GameManager.getInstance().gadget.screenRecorder.CaptureScreenshot();
        GameManager.getInstance().gadget.gadgetCamera.enabled = false;
        GameManager.getInstance().gadget.xrCamera.enabled = true;


        // TODO CRITICAL BUG, COUNT is NOT ENOUGH to know which texture is for style and which for content, need another factor to figure them out.
        // Content texture is the first one in the uploadTextures List,
        // if there is already one, replace it with the current one, if there
        /*if (diffusionRequest.uploadTextures.Count >= 2)
        {
            diffusionRequest.uploadTextures[1] = curTexture;
        }
        diffusionRequest.uploadImage = screenShot;*/

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
