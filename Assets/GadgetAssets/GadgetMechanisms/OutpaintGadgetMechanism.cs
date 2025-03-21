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

    // Diffusable Object that is being held up
    private GameObject grabbedObject;

    // These are used to represent the tiles that are under progress
    private Texture2D[] NOISE_TEXTURES;

    private void Awake()
    {
        mechanismText = "Outpainting";
        NOISE_TEXTURES = Resources.LoadAll<Texture2D>("Textures/Noise");
    }

    // TODO currently the mechanism will work by CLICKING on a DiffusableObject,
    // TODO getting the keywords from it and then picking a relevant OutpaintingTile to start the generation on

    /// <summary>
    /// Helper function for the Outpainting Mechanism script that checks whether a interactable object should be interacted with further.
    /// </summary>
    /// <param name="args">Interactable Object args to check</param>
    /// <returns>True if should be interacted with</returns>
    private bool ValidInteractableObject(BaseInteractionEventArgs args, bool grabbable)
    {
        if (args == null || args.interactableObject == null) return false;
        if (GameManager.getInstance() == null) return false;

        Transform curTrans = args.interactableObject.transform;

        // When you use a GrabInteractable, it moves the transform in the hierarchy, thus not being in the Diffusables anymore while being grabbed.
        if (!grabbable)
        {
            if (curTrans.parent != GameManager.getInstance().diffusables.transform) return false;
        }
        if (curTrans.gameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            if (!DO.Model3D) return false;
        }

        return true;
    }


    public override void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        if (grabbedObject == null) return;
        if (!ValidInteractableObject(args, false)) return;

        if (args.interactableObject.transform.gameObject.TryGetComponent<OutpaintingTile>(out OutpaintingTile OPT))
        {
            if (!OPT.paintable || OPT.painted) return;

            // Creates pre-selection outline
            GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.preSelected);
        }  
    }

    public override void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        if (!ValidInteractableObject(args, false)) return;

        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);
    }


    public override void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (!ValidInteractableObject(args, true)) return;

        if (args.interactableObject.transform.gameObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            if (DO.Model3D)
            {
                grabbedObject = args.interactableObject.transform.gameObject;
            }
        }        
    }
    // TODO notice possible issue, you grab with left, then right, then release left, you get null, not right
    // TODO weird potential problem
    // TODO  - solved when right calls above function and then right becomes grabbed?
    public override void DiffusableUnGrabbed(SelectExitEventArgs args)
    { 
        if (!ValidInteractableObject(args, true)) return;
        if (args.interactableObject.transform.gameObject != grabbedObject) return;
        grabbedObject = null;
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
        newDiffusionRequest.addToTextureTotal = false;

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
            Texture2D curTexture = null;
            if (curOffsetTile.TryGetComponent<TextureTransition>(out TextureTransition TT))
            {
                if (TT.textures.Count <= 0)
                {
                    Debug.LogError("There is no texture in the painted tile " + curOffsetTile.name);
                    return false;
                }

                Texture curTextureToConvert = TT.textures[0];
                curTexture = GeneralGameLibraries.TextureManipulationLibrary.toTexture2D(curTextureToConvert);
            }
            else
            {
                curTexture = TextureManipulationLibrary.toTexture2D(curOffsetTile.GetComponent<Renderer>().material.mainTexture);
            }

            diffusionRequest.uploadTextures.Add(curTexture);
            curTexture.name = mainTileName + "_" + offsetTileName + ".png";
            diffusionRequest.SpecialInput = offsetTileName;

            return true;
        }

        return false;
    }

    
    /// <summary>
    /// Helper function that gets a TextureTransition and adds a "in-progress" effect with a few random textures
    /// </summary>
    private void CreateInProgressDiffusionTileEffect(TextureTransition TT)
    {
        List<Texture2D> listTextures = new List<Texture2D>(NOISE_TEXTURES);
        listTextures = GeneralGameLibraries.GetRandomElements(listTextures, 2);
        if (listTextures.Count <= 1)
        {
            Debug.LogError("Add more noise textures to the Resources/Textures/Noise folder");
        }
        TT.TransitionTextures(new List<Texture> { listTextures[0], listTextures[1] }, -1, 0, 1);
    }

    public override void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        if (grabbedObject == null) return;
        if (!ValidInteractableObject(args, false)) return;

        string curPositivePrompt = "";
        if (grabbedObject.TryGetComponent<DiffusableObject>(out DiffusableObject DO))
        {
            if (!DO.Model3D) return;
            curPositivePrompt = DO.keyword;
        }

        DiffusionRequest newDiffusionRequest = CreateDiffusionRequest();
        newDiffusionRequest.positivePrompt = curPositivePrompt;

        OutpaintingTile OPT = args.interactableObject.transform.gameObject.GetComponent<OutpaintingTile>();
        TextureTransition TT = args.interactableObject.transform.gameObject.GetComponent<TextureTransition>();

        if (OPT == null || TT == null) return;

        // Object that is interacted with is an OutpaintingTile
        if (!(OPT.paintable && !OPT.painted)) return;

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
           /* if (newDiffusionRequest.uploadTextures.Count <= 1)
            {*/
                rightTileOutpaint = CheckAdjacentTile(new Vector2Int(1, 0), newDiffusionRequest, OPT, uniqueName, "right");
            //}
        }                      

        if (!topTileOutpaint && !leftTileOutpaint && !rightTileOutpaint)    return;
        if (topTileOutpaint && leftTileOutpaint)
        {
            if (!CheckAdjacentTile(new Vector2Int(-1, 1), newDiffusionRequest, OPT, uniqueName, "bottomRight")) return;
            newDiffusionRequest.diffusionJsonType = diffusionWorkflows.grid4Outpainting;
        }
        else if (topTileOutpaint && rightTileOutpaint)
        {
            if (!CheckAdjacentTile(new Vector2Int(1, 1), newDiffusionRequest, OPT, uniqueName, "bottomLeft")) return;
            newDiffusionRequest.diffusionJsonType = diffusionWorkflows.grid4Outpainting;
        }

        CreateInProgressDiffusionTileEffect(TT);
        newDiffusionRequest.targets.Add(TT);        

        ObjectFlightToTile curFlight = grabbedObject.AddComponent<ObjectFlightToTile>();
        curFlight.StartMovement(grabbedObject.transform.position, args.interactableObject.transform.position);

        GameManager.getInstance().gadget.ChangeOutline(args.interactableObject.transform.gameObject, GadgetSelection.unSelected);

        XRGrabInteractable xRGrabInteractable = grabbedObject.GetComponent<XRGrabInteractable>();
        if (xRGrabInteractable != null) xRGrabInteractable.enabled = false;
        grabbedObject = null;

        GameManager.getInstance().gadget.playGadgetSounds.PlaySound("SelectElement");

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(newDiffusionRequest);
    }
}