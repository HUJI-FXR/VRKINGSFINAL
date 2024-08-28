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


    private void Start()
    {
        mechanismText = "Throwing";
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Throwing Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest()
    {
        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.diffusionModel = diffusionModels.nano;
        newDiffusionRequest.targets.Add(GameManager.getInstance().radiusDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.txt2imgLCM;
        newDiffusionRequest.numOfVariations = 5;

        return newDiffusionRequest;
    }

    public void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if(args.interactableObject == null)
        {
            return;
        }
        
        DiffusionRequest diffusionRequest = CreateDiffusionRequest();

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