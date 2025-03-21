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
    private void Awake()
    {
        mechanismText = "Throwing";
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for the Throwing Mechanism
    /// </summary>
    /// <returns></returns>
    protected override DiffusionRequest CreateDiffusionRequest()
    {
        if (GameManager.getInstance() == null) return null;

        DiffusionRequest newDiffusionRequest = new DiffusionRequest();

        newDiffusionRequest.diffusionModel = diffusionModels.nano;
        newDiffusionRequest.targets.Add(GameManager.getInstance().radiusDiffusionTexture);
        newDiffusionRequest.diffusionJsonType = diffusionWorkflows.txt2imgLCM;
        newDiffusionRequest.numOfVariations = 5;

        return newDiffusionRequest;
    }

    public override void DiffusableGrabbed(SelectEnterEventArgs args)
    {
        if (GameManager.getInstance() == null) return;
        if(args.interactableObject == null) return;
        
        DiffusionRequest diffusionRequest = CreateDiffusionRequest();

        diffusionRequest.diffusableObject = args.interactableObject.transform.gameObject.GetComponent<DiffusableObject>();
        diffusionRequest.positivePrompt = diffusionRequest.diffusableObject.keyword;

        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);        
    }
    public override void DiffusableUnGrabbed(SelectExitEventArgs args)
    {
        if (args.interactableObject == null) return;

        if (args.interactableObject.transform.gameObject.TryGetComponent<ParticleSystem>(out ParticleSystem ps))
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }
}