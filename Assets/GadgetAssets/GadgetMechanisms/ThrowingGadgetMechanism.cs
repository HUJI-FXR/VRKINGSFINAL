using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;

public class ThrowingGadgetMechanism : GadgetMechanism
{
    // TODO Do I even need diffusionlist when I have  GeneralGameScript.instance.diffusables??

    //private bool allowCollision = false;
    public DiffusionRequest diffusionRequest = null;


    private void Start()
    {
        mechanismText = "Throwing";
    }

    /*
    diffusionRequest.positivePrompt = "Beautiful";
    diffusionRequest.negativePrompt = "watermark";
    diffusionRequest.numOfVariations = 5;
    diffusionRequest.targets.Add(GameManager.getInstance().radiusDiffusionTexture);
    diffusionRequest.diffusionModel = diffusionModels.nano;*/

    /*public ThrowingGadgetMechanism()
    {
        this.mechanismText = MECHANISM_PRETEXT + "Throw an Object";
        this.buttonText = "Generate"; // TODO deccide if throwing mechanism is per object or from the gadget, this will determine button text too

        diffusionRequest = new DiffusionRequest();
        diffusionRequest.positivePrompt = "Beautiful";
        diffusionRequest.negativePrompt = "watermark";
        diffusionRequest.numOfVariations = 5;
        diffusionRequest.targets.Add(GameManager.getInstance().radiusDiffusionTexture);
        diffusionRequest.diffusionModel = diffusionModels.nano;
    }*/

    public void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if(args.interactableObject == null)
        {
            return;
        }
        diffusionRequest.diffusableObject = args.interactableObject.transform.gameObject.GetComponent<DiffusableObject>();
        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);        
    }
    public void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        if (args.interactableObject == null)
        {
            return;
        }

        if (args.interactableObject.transform.gameObject.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }
}