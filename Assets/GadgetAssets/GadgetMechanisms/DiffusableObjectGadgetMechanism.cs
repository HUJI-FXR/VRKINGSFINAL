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
// TODO make requests repeat themselves until completion with a max number of RETRIES


public class DiffusableObjectGadgetMechanism : GadgetMechanism
{
    [NonSerialized]
    public GameObject selectedStyleObject = null;
    [NonSerialized]
    public GameObject selectedTextObject = null;

    public DiffusionRequest diffusionRequest;

    private void Start()
    {
        mechanismText = "Object to Image";
    }

    // In this Mechanism, the selection of images happens equally with both hands.
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

    private bool validInteractableObject(BaseInteractionEventArgs args)
    {
        if (args == null || args.interactableObject == null) return false;

        Transform curTransform = args.interactableObject.transform;

        if (curTransform.parent != GameManager.getInstance().diffusables.transform) return false;
        if (curTransform.GetComponent<DiffusableObject>() == null &&
            curTransform.GetComponent<Renderer>().material.mainTexture == null) return false;

        return true;
    }

    public void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (!validInteractableObject(args))
        {
            Debug.Log(args.interactableObject.transform.gameObject.name);
            return;
        }        

        // Creates pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!validInteractableObject(args)) return;

        // Remove pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (!validInteractableObject(args)) return;

        GameObject curInteractable = args.interactableObject.transform.gameObject;
        DiffusableObject curDO = curInteractable.GetComponent<DiffusableObject>();

        if (curDO != null)
        {
            if (selectedTextObject != null)
            {
                GameManager.getInstance().gadget.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);
                selectedTextObject = null;
            }

            selectedTextObject = curInteractable;
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);

            return;
        }

        if (curInteractable.GetComponent<Renderer>().material.mainTexture == null)  return;

        // Adds to queue of selected objects
        if (selectedStyleObject != null)
        {
            GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
            selectedStyleObject = null;
        }

        selectedStyleObject = curInteractable;

        // Creates selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
    }    

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public override void PlaceTextureInput(InputAction.CallbackContext context)
    {
        Texture2D curTexture = GameManager.getInstance().gadget.getGeneratedTexture();
        if (curTexture == null)
        {
            Debug.Log("Tried to add a textures from the Gadget camera without textures in the Queue");
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
        if (selectedTextObject == null || selectedStyleObject == null)  return;

        Texture styleTexture = selectedStyleObject.GetComponent<Renderer>().material.mainTexture;
        string positivePrompt = selectedTextObject.GetComponent<DiffusableObject>().keyword;

        GameManager.getInstance().gadget.ChangeOutline(selectedStyleObject, GadgetSelection.unSelected);
        GameManager.getInstance().gadget.ChangeOutline(selectedTextObject, GadgetSelection.unSelected);

        Texture2D copyStyleTexture = TextureManipulationLibrary.toTexture2D(styleTexture);

        string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
        copyStyleTexture.name = uniqueName + ".png";

        diffusionRequest.uploadImage = copyStyleTexture;
        diffusionRequest.positivePrompt = positivePrompt;

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
    }
}