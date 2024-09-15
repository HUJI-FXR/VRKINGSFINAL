using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;
using System;


// TODO make a unique ID for everything, downloaded images, uploaded images, request IDs etc


public class DiffusableObjectGadgetMechanism : GadgetMechanism
{
    [NonSerialized]
    public GameObject selectedStyleObject = null;
    [NonSerialized]
    public GameObject selectedTextObject = null;

    private void Start()
    {
        mechanismText = "Object to Image";
    }
    
    private bool validInteractableObject(BaseInteractionEventArgs args)
    {
        if (GameManager.getInstance() == null) return false;
        if (args == null || args.interactableObject == null) return false;

        Transform curTransform = args.interactableObject.transform;

        if (curTransform.parent != GameManager.getInstance().diffusables.transform) return false;
        if (curTransform.GetComponent<DiffusableObject>() == null &&
            curTransform.GetComponent<Renderer>().material.mainTexture == null) return false;

        return true;
    }

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if (!validInteractableObject(args))
        {
            Debug.Log(args.interactableObject.transform.gameObject.name);
            return;
        }
        if (selectedStyleObject == args.interactableObject.transform.gameObject || selectedTextObject == args.interactableObject.transform.gameObject) return;

        // Creates pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!validInteractableObject(args)) return;
        if (selectedStyleObject == args.interactableObject.transform.gameObject || selectedTextObject == args.interactableObject.transform.gameObject) return;

        // Remove pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if (!validInteractableObject(args)) return;

        GameObject curInteractable = args.interactableObject.transform.gameObject;
        DiffusableObject curDO = curInteractable.GetComponent<DiffusableObject>();

        if (curDO != null)
        {
            if (curDO.Model3D)
            {
                if (selectedTextObject != null)
                {
                    GameManager.getInstance().gadget.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
                }
                selectedTextObject = args.interactableObject.transform.gameObject;
            }
            else
            {
                if (selectedStyleObject != null)
                {
                    GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
                }
                selectedStyleObject = args.interactableObject.transform.gameObject;
            }

            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
            return;
        }

        if (curInteractable.GetComponent<Renderer>().material.mainTexture == null)  return;
        selectedStyleObject = args.interactableObject.transform.gameObject;

        GameManager.getInstance().gadget.playGadgetSounds.PlaySound("SelectElement");

        // Creates selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
    }    

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public override void PlaceTextureInput(GameObject GO)
    {
        if (GameManager.getInstance() == null) return;
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
                GameManager.getInstance().gadget.playGadgetSounds.PlaySound("ImagePlacement");

                dtc.AddTexture(new List<Texture2D>() { curTexture }, false);
            }
        }
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Diffusable Object Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest()
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.img2imgLCM;

        return newDiffusionRequest;
    }

    public override void ActivateGeneration(GameObject GO)
    {
        if (GameManager.getInstance() == null) return;
        if (selectedTextObject == null || selectedStyleObject == null)  return;

        Texture styleTexture = selectedStyleObject.GetComponent<Renderer>().material.mainTexture;
        string positivePrompt = selectedTextObject.GetComponent<DiffusableObject>().keyword;

        Texture2D copyStyleTexture = TextureManipulationLibrary.toTexture2D(styleTexture);

        string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
        copyStyleTexture.name = uniqueName + ".png";

        DiffusionRequest diffusionRequest = CreateDiffusionRequest();

        diffusionRequest.uploadTextures.Add(copyStyleTexture);
        diffusionRequest.positivePrompt = positivePrompt;

        GameManager.getInstance().gadget.playGadgetSounds.PlaySound("ImagePlacement");

        ResetMechanism();

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }

    public override void ResetMechanism()
    {
        if (selectedTextObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
            selectedTextObject = null;
        }

        if (selectedStyleObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }
    }
}