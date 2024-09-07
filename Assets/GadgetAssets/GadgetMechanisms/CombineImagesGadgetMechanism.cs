using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Linq;


// TODO make a unique ID for everything, downloaded images, uploaded images, request IDs etc
public class CombineImagesGadgetMechanism : GadgetMechanism
{
    // TODO notice this CAN be generalized to larger number of objects, but requires many changes.
    public Queue<GameObject> selectedObjects = new Queue<GameObject>();
    public int MAX_QUEUED_OBJECTS = 2;
    public UnityEvent unityEvent;

    private void Start()
    {
        mechanismText = "Combine Images";
    }

    /// <summary>
    /// Helper function for the Combine Mechanism script that checks whether a interactable object should be interacted with further.
    /// </summary>
    /// <param name="args">Interactable Object args to check</param>
    /// <returns>True if should be interacted with</returns>
    private bool ValidInteractableObject(BaseInteractionEventArgs args)
    {
        if (args == null || args.interactableObject == null) return false;
        if (GameManager.getInstance() == null) return false;
        Transform curTrans = args.interactableObject.transform;
        if (curTrans.parent != GameManager.getInstance().diffusables.transform) return false;
        if (curTrans.gameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            if (DO.Model3D) return false;
        }
        if (selectedObjects.Contains(curTrans.gameObject)) return false;
        if (curTrans.gameObject.TryGetComponent<Renderer>(out Renderer REN))
        {
            if (REN.material.mainTexture == null) return false;
        }
        else
        {
            return false;
        }

        return true;
    }

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (!ValidInteractableObject(args)) return;

        // Creates pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!ValidInteractableObject(args)) return;

        // Remove pre-selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (!ValidInteractableObject(args)) return;

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
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        newDiffusionRequest.targets.Add(GameManager.getInstance().uiDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.combineImages;
        newDiffusionRequest.numOfVariations = 1;

        return newDiffusionRequest;
    }

    public void GetTexturesFromSelected()
    {
        if (GameManager.getInstance() == null) return;
        if (selectedObjects.Count != MAX_QUEUED_OBJECTS) return;

        GameObject firstGameObject = selectedObjects.Dequeue();
        GameObject secondGameObject = selectedObjects.Dequeue();
        Texture go1Text = firstGameObject.GetComponent<Renderer>().material.mainTexture;
        Texture go2Text = secondGameObject.GetComponent<Renderer>().material.mainTexture;        

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

        ResetMechanism();

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
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
            // TODO discuss with NADAV
            if (hit.collider.gameObject.TryGetComponent<DiffusionTextureChanger>(out DiffusionTextureChanger dtc))
            {
                dtc.AddTexture(new List<Texture2D>() { curTexture }, false);

                // Sending broadcast to Game timeline script
                unityEvent?.Invoke();
            }
        }
    }
    public override void ActivateGeneration(GameObject GO)
    {
        GetTexturesFromSelected();
    }

    public override void ResetMechanism()
    {
        foreach(GameObject GO in selectedObjects)
        {
            GameManager.getInstance().gadget.ChangeOutline(GO, GadgetSelection.unSelected);
        }

        selectedObjects = new Queue<GameObject>();
    }
}