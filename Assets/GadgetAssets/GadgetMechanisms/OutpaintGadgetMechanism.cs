using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;


// TODO need to remove button from several of these, choose exactly
public class OutpaintGadgetMechanism : GadgetMechanism
{
    public DiffusionRequest diffusionRequest;
    private string currentKeywords = "";

    // TODO currently the mechanism will work by CLICKING on a DiffusableObject, getting the keywords from it and then picking a relevant OutpaintingTile to start the generation on.outpaintin

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

        DiffusableObject diffObj = args.interactableObject.transform.gameObject.GetComponent<DiffusableObject>();
        OutpaintingTile OPT = args.interactableObject.transform.gameObject.GetComponent<OutpaintingTile>();
        if (diffObj == null && OPT == null) 
        {
            return;
        }

        if (OPT != null)
        {            
            if (OPT.paintable && !OPT.painted)
            {

            }
        }
        else
        {
            currentKeywords = diffObj.keyword;
        }

        // Creates selection outline
        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);
        Texture2D curTexture = TextureManipulationLibrary.toTexture2D(args.interactableObject.transform.gameObject.GetComponent<Renderer>().material.mainTexture);
        diffusionRequest.secondUploadImage = curTexture;
    }

    public override void GeneralActivation(DiffusionTextureChanger dtc)
    {
        diffusionRequest.targets.Add(dtc);
        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        return;
    }
}