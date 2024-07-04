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
    public DiffusionRequest diffusionRequest = null;

    public override void GeneralActivation(DiffusionTextureChanger dtc)
    {
        diffusionRequest.targets.Add(dtc);
        GameManager.getInstance().comfyOrganizer.SendDiffusionRequest(diffusionRequest);
        return;
    }
}