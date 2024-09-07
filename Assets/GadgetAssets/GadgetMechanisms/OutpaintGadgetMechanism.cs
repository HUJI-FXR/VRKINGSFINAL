using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;
using static UnityEngine.XR.Hands.XRHandTrackingEvents;


public class OutpaintGadgetMechanism : GadgetMechanism
{
    public OutpaintingScreenScr outpaintingScreen;
    private string curKeyword;

    private void Start()
    {
        mechanismText = "Outpainting";
    }

    // TODO currently the mechanism will work by CLICKING on a DiffusableObject, getting the keywords from it and then picking a relevant OutpaintingTile to start the generation on.outpaintin

    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if (args == null || args.interactableObject == null) return;
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform) return;

        OutpaintingTile OPT = args.interactableObject.transform.gameObject.GetComponent<OutpaintingTile>();
        if (OPT != null)
        {
            if (!OPT.paintable || OPT.painted) return;

            // Creates pre-selection outline
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
        }        
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if (args == null || args.interactableObject == null) return;
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform) return;

        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }


    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Outpainting Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest()
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        //newDiffusionRequest.diffusionModel = diffusionModels.ghostmix;
        newDiffusionRequest.diffusionModel = diffusionModels.juggernautXLInpaint;
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.outpainting;

        return newDiffusionRequest;
    }


    /// <summary>
    /// Helper function for the selection function. 
    /// Allows to determine if the adjacent tile that is given, could be used in the outpainting process as an input. 
    /// </summary>
    /// <param name="offset">Determines which tile is checked of the surrounding tiles</param>
    /// <param name="diffusionRequest">DiffusionRequest to change</param>
    /// <param name="OPT">Input tile for which we check its adjacents</param>
    /// <param name="offsetTileName">Adjacent tile name</param>
    /// <param name="mainTileName">Input tile name</param>
    /// <returns> true if the adjacent tile that is given, could be used in the outpainting process as an input, 
    /// false otherwise</returns>
    private bool CheckAdjacentTile(Vector2Int offset, DiffusionRequest diffusionRequest, OutpaintingTile OPT, string mainTileName, string offsetTileName)
    {
        GameObject curOffsetTile = outpaintingScreen.tiles[OPT.tilePosition.x + offset.x, OPT.tilePosition.y + offset.y];
        if (curOffsetTile == null) return false;
        OutpaintingTile curTileComp = curOffsetTile.GetComponent<OutpaintingTile>();
        if (curTileComp == null) return false;

        if (curTileComp.painted == true)
        {
            Texture2D curTexture = TextureManipulationLibrary.toTexture2D(curOffsetTile.GetComponent<Renderer>().material.mainTexture);
            diffusionRequest.uploadTextures.Add(curTexture);
            curTexture.name = mainTileName + "_" + offsetTileName + ".png";
            diffusionRequest.SpecialInput = offsetTileName;

            return true;
        }

        return false;
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if (args == null || args.interactableObject == null) return;
        if (args.interactableObject.transform.parent != GameManager.getInstance().diffusables.transform) return;

        DiffusionRequest newDiffusionRequest = CreateDiffusionRequest();

        DiffusableObject diffObj = args.interactableObject.transform.gameObject.GetComponent<DiffusableObject>();
        OutpaintingTile OPT = args.interactableObject.transform.gameObject.GetComponent<OutpaintingTile>();
        RegularDiffusionTexture RDT = args.interactableObject.transform.gameObject.GetComponent<RegularDiffusionTexture>();
        if (diffObj == null) 
        {
            if (OPT == null) return;

            // Object that is interacted with is an OutpaintingTile
            if (!(OPT.paintable && !OPT.painted) || RDT == null) return;

            // Finding a texture to be the original to be outpainted from.
            bool topTileOutpaint = false;
            bool leftTileOutpaint = false;
            bool rightTileOutpaint = false;

            string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();

            // Top tile outpainting
            if (OPT.tilePosition.y < outpaintingScreen.tileMatrixSize.y - 1)
            {
                topTileOutpaint = CheckAdjacentTile(new Vector2Int(0, 1), newDiffusionRequest, OPT, uniqueName, "top");
            }

            // Left tile outpainting
            if (OPT.tilePosition.x > 0)
            {
                leftTileOutpaint = CheckAdjacentTile(new Vector2Int(-1, 0), newDiffusionRequest, OPT, uniqueName, "left");
            }

            // Right tile outpainting
            if (OPT.tilePosition.x < outpaintingScreen.tileMatrixSize.x - 1)
            {
                if (newDiffusionRequest.uploadTextures.Count <= 1)
                {
                    rightTileOutpaint = CheckAdjacentTile(new Vector2Int(1, 0), newDiffusionRequest, OPT, uniqueName, "right");
                }
            }                      

            if (!topTileOutpaint && !leftTileOutpaint && !rightTileOutpaint)    return;
            if (topTileOutpaint && leftTileOutpaint)
            {
                CheckAdjacentTile(new Vector2Int(-1, 1), newDiffusionRequest, OPT, uniqueName, "bottomRight");
                newDiffusionRequest.diffusionJsonType = diffusionWorkflows.grid4Outpainting;
            }
            else if (topTileOutpaint && rightTileOutpaint)
            {
                CheckAdjacentTile(new Vector2Int(1, 1), newDiffusionRequest, OPT, uniqueName, "bottomLeft");
                newDiffusionRequest.diffusionJsonType = diffusionWorkflows.grid4Outpainting;
            }

            outpaintingScreen.UpdateTiles(new Vector2Int(OPT.tilePosition.x, OPT.tilePosition.y));                               
            newDiffusionRequest.targets.Add(RDT);

            newDiffusionRequest.positivePrompt = curKeyword;

            GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(newDiffusionRequest);
        }
        // Object that is interacted with is a DiffusableObject
        else
        {
            curKeyword = diffObj.keyword;

            // Creates selection outline
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);         
        }                        
    }
}