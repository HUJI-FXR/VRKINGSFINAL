using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;
using static UnityEngine.XR.Hands.XRHandTrackingEvents;


// TODO need to remove button from several of these, choose exactly
public class OutpaintGadgetMechanism : GadgetMechanism
{
    public DiffusionRequest diffusionRequest;
    public OutpaintingScreenScr outpaintingScreen;

    private void Start()
    {
        mechanismText = "Outpainting";
    }

    // TODO currently the mechanism will work by CLICKING on a DiffusableObject, getting the keywords from it and then picking a relevant OutpaintingTile to start the generation on.outpaintin

    public override void OnGameObjectLeftHoverEntered(HoverEnterEventArgs args)
    {
        if (args == null || args.interactableObject == null)
        {
            return;
        }
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform)
        {
            return;
        }

        OutpaintingTile OPT = args.interactableObject.transform.gameObject.GetComponent<OutpaintingTile>();
        if (OPT != null)
        {
            if (!OPT.paintable || OPT.painted)
            {
                return;
            }

            // Creates pre-selection outline
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
        }        
    }

    public override void OnGameObjectLeftHoverExited(HoverExitEventArgs args)
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

    public override void onGameObjectLeftSelectEntered(SelectEnterEventArgs args)
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

            // Top tile outpainting
            if (OPT.tilePosition.y < outpaintingScreen.tileMatrixSize.y-1 && 
                outpaintingScreen.tiles[OPT.tilePosition.x, OPT.tilePosition.y+1].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x, OPT.tilePosition.y + 1];
                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                diffusionRequest.SpecialInput = "top";
            }

            // Left tile outpainting
            else if (OPT.tilePosition.x > 0 && 
                outpaintingScreen.tiles[OPT.tilePosition.x-1, OPT.tilePosition.y].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x-1, OPT.tilePosition.y];
                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                diffusionRequest.SpecialInput = "left";
            }

            // Right tile outpainting
            else if (OPT.tilePosition.x < outpaintingScreen.tileMatrixSize.x-1 && 
                outpaintingScreen.tiles[OPT.tilePosition.x+1, OPT.tilePosition.y].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x+1, OPT.tilePosition.y];
                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                diffusionRequest.SpecialInput = "right";
            }
            else
            {
                return;
            }

            outpaintingScreen.UpdateTiles(new Vector2Int(OPT.tilePosition.x, OPT.tilePosition.y));

            diffusionRequest.uploadImage = curTexture;
            string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();
            curTexture.name = uniqueName + ".png";

            if (diffusionRequest.targets.Count > 0)
            {
                diffusionRequest.targets.Clear();
            }
            diffusionRequest.targets.Add(RDT);

            GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);

        }
        // Object that is interacted with is a DiffusableObject
        else
        {
            // Creates selection outline
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);

            diffusionRequest.positivePrompt = diffObj.keyword;
        }                        
    }
}