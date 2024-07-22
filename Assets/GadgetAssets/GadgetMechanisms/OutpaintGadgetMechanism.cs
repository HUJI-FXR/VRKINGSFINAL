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
    public OutpaintingScreenScr outpaintingScreen;
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
        RegularDiffusionTexture RDT = args.interactableObject.transform.gameObject.GetComponent<RegularDiffusionTexture>();
        if (diffObj == null) 
        {
            if (OPT == null)
            {
                return;
            }
            // Object that is interacted with is an OutpaintingTile
            if (!(OPT.paintable && !OPT.painted) || RDT == null)
            {
                return;
            }

            Texture2D curTexture;

            // Finding a texture to be the original to be outpainted from.
            GameObject curTileGO;
            if (OPT.tilePosition.y < outpaintingScreen.tileMatrixSize.y-1 && 
                outpaintingScreen.tiles[OPT.tilePosition.x, OPT.tilePosition.y+1].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x, OPT.tilePosition.y + 1];
                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
            }
            else if (OPT.tilePosition.x > 0 && 
                outpaintingScreen.tiles[OPT.tilePosition.x-1, OPT.tilePosition.y].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x-1, OPT.tilePosition.y];
                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
            }
            else if(OPT.tilePosition.x < outpaintingScreen.tileMatrixSize.x-1 && 
                outpaintingScreen.tiles[OPT.tilePosition.x+1, OPT.tilePosition.y].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x+1, OPT.tilePosition.y];
                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
            }
            else
            {
                return;
            }

            Debug.Log("trial2");

            diffusionRequest.uploadImage = curTexture;
            diffusionRequest.targets.Add(RDT);
            GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        }
        // Object that is interacted with is a DiffusableObject
        else
        {
            Debug.Log("trial3 " + diffObj.keyword);
            // Creates selection outline
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);

            currentKeywords = diffObj.keyword;
        }                        
    }

    public override void GeneralActivation(DiffusionTextureChanger dtc)
    {
        diffusionRequest.targets.Add(dtc);
        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        return;
    }
}