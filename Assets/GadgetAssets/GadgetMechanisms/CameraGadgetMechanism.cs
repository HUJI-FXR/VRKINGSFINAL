using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;

/// <summary>
/// Gadget Mechanism wherein a Camera is used to screenshot within the game world,
/// And that screenshot is used as an input in an img2img Diffusion workflow.
/// </summary>
public class CameraGadgetMechanism : GadgetMechanism
{
    // Texture chosen to be used as the style component in the combination with the content texture
    private Texture2D styleTexture;

    // Screenshot that was taken with the Camera
    private Texture2D contentTexture;
    public bool UseStyleTransfer = true;

    // Limits when a picture can be taken
    public bool takePicture = false;

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
    public override void PlaceTextureInput(GameObject GO)
    {
        if (takePicture) return;
        if (GO == null) return;

        Texture2D curTexture = GameManager.getInstance().gadget.getGeneratedTexture();
        if (curTexture == null)
        {
            Debug.LogError("Tried to add a textures from the Gadget camera without textures in the Queue");
            return;
        }

        // Perform the raycast
        // Ray ray = new Ray(GameManager.getInstance().gadget.transform.position, GameManager.getInstance().gadget.transform.right);
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

        string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
        curTexture.name = uniqueName + "_2.png";

        styleTexture = curTexture;
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Camera Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest()
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);

        // TODO do I even NEED the baseCamera workflow? diffusionWorkflows.baseCamera
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;

        return newDiffusionRequest;
    }


    /// <summary>
    /// Activates the Diffusion image generation using the right hand controller. 
    /// For the generation to begin, a camera shot and a style texture need to be picked to be sent to the generator.
    /// </summary>
    public override void ActivateGeneration(GameObject GO)
    {
        if (contentTexture == null || styleTexture== null)
        {
            Debug.LogError("Need to pick style and content textures for Camera mechanism");
            return;
        }

        if (selectedStyleObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }


        // Content texture is the first one in the uploadTextures List, Style texture is the second.
        DiffusionRequest newDiffusionRequest = CreateDiffusionRequest();
        newDiffusionRequest.uploadTextures.Add(contentTexture);
        newDiffusionRequest.uploadTextures.Add(styleTexture);

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(newDiffusionRequest);
        return;
    }

    // This function is called when the grabbed camera is activated(a screenshot is taken).
    /// <summary>
    /// Shoots an image with the Physical grabbable Camera.
    /// </summary>
    public override void TakeScreenshot(Texture2D screenShot, Camera camera)
    {
        // TODO add DiffusableObject data entry for diffusionrequest when taking a picture of stuff        
        camera.enabled = false;
        GameManager.getInstance().gadget.xrCamera.enabled = true;

        contentTexture = screenShot;

        GameManager.getInstance().gadget.playGadgetSounds.PlaySound("cameraShutter");

        if (selectedStyleObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }

        // TODO do I need this part? why do I want to send a DiffusionRequest when taking a picture with the camera??
        /*if (!UseStyleTransfer)
        {
            diffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;
            GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        }*/
    }
}
