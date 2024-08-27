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

        DiffusionRequest newDiffusionRequest = GameManager.getInstance().comfyOrganizer.copyDiffusionRequest(diffusionRequest);

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

            bool topTileOutpaint = false;
            bool leftTileOutpaint = false;
            bool rightTileOutpaint = false;

            string uniqueName = GameManager.getInstance().comfyOrganizer.UniqueImageName();

            // Top tile outpainting
            if (OPT.tilePosition.y < outpaintingScreen.tileMatrixSize.y-1 && 
                outpaintingScreen.tiles[OPT.tilePosition.x, OPT.tilePosition.y+1].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x, OPT.tilePosition.y + 1];

                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                newDiffusionRequest.uploadTextures.Add(curTexture);
                curTexture.name = uniqueName + "_top" + ".png";

                newDiffusionRequest.SpecialInput = "top";
                topTileOutpaint = true;
            }

            // Left tile outpainting
            else if (OPT.tilePosition.x > 0 && 
                outpaintingScreen.tiles[OPT.tilePosition.x-1, OPT.tilePosition.y].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x-1, OPT.tilePosition.y];

                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                newDiffusionRequest.uploadTextures.Add(curTexture);
                curTexture.name = uniqueName + "_left" + ".png";

                newDiffusionRequest.SpecialInput = "left";
                leftTileOutpaint = true;
            }

            // Right tile outpainting
            else if (OPT.tilePosition.x < outpaintingScreen.tileMatrixSize.x-1 && 
                outpaintingScreen.tiles[OPT.tilePosition.x+1, OPT.tilePosition.y].GetComponent<OutpaintingTile>().painted == true)
            {
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x+1, OPT.tilePosition.y];

                if (newDiffusionRequest.uploadTextures.Count <= 1)
                {
                    curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                    newDiffusionRequest.uploadTextures.Add(curTexture);
                    curTexture.name = uniqueName + "_right" + ".png";
                }                

                newDiffusionRequest.SpecialInput = "right";
                rightTileOutpaint = true;
            }
            else
            {
                return;
            }

            // TODO check if edge texture exists two, like, the third image
            // TODO repeat code, bad!
            if (topTileOutpaint && leftTileOutpaint)
            {
                newDiffusionRequest.diffusionJsonType = diffusionWorkflows.grid4Outpainting;
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x-1, OPT.tilePosition.y+1];

                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                newDiffusionRequest.uploadTextures.Add(curTexture);
                curTexture.name = uniqueName + "_bottomRight" + ".png";
            }
            else if (topTileOutpaint && rightTileOutpaint)
            {
                newDiffusionRequest.diffusionJsonType = diffusionWorkflows.grid4Outpainting;
                curTileGO = outpaintingScreen.tiles[OPT.tilePosition.x+1, OPT.tilePosition.y+1];

                curTexture = TextureManipulationLibrary.toTexture2D(curTileGO.GetComponent<Renderer>().material.mainTexture);
                newDiffusionRequest.uploadTextures.Add(curTexture);
                curTexture.name = uniqueName + "_bottomLeft" + ".png";
            }

            outpaintingScreen.UpdateTiles(new Vector2Int(OPT.tilePosition.x, OPT.tilePosition.y));                               
            newDiffusionRequest.targets.Add(RDT);

            GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(newDiffusionRequest);

        }
        // Object that is interacted with is a DiffusableObject
        else
        {
            // TODO problem with new and old diffusion requests, need to connect the selected object with the later generation.
            newDiffusionRequest.positivePrompt = diffObj.keyword;

            // Creates selection outline
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.selected);         
        }                        
    }
}