using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;


// TODO need to remove button from several of these, choose exactly

/// <summary>
/// Represents a Gadget(Diffusion images making device) Mechanism, using a Strategy design pattern.
/// Other Gadget Mechanisms inherit from this one.
/// </summary>
public class GadgetMechanism : MonoBehaviour
{
    public static string MECHANISM_PRETEXT = "Mechanism:\n";

    /// <summary>
    /// Text that will be shown that represents and indicates the mechanism.
    /// </summary>
    public string mechanismText;

    // TODO delete this one??
    public string buttonText;

    public GadgetMechanism()
    {
        this.mechanismText = "";
        this.buttonText = "";
    }

    public virtual void OnGameObjectLeftHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    public virtual void OnGameObjectLeftHoverExited(HoverExitEventArgs args)
    {
        return;
    }

    public virtual void OnGameObjectRightHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    public virtual void OnGameObjectRightHoverExited(HoverExitEventArgs args)
    {
        return;
    }

    public virtual void onGameObjectLeftSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    public virtual void onGameObjectLeftSelectExited(SelectExitEventArgs args)
    {
        return;
    }
    public virtual void onGameObjectRightSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    public virtual void onGameObjectRightSelectExited(SelectExitEventArgs args)
    {
        return;
    }

    /*public virtual void OnUIHoverEntered(UIHoverEventArgs args)
    {
        return;
    }
    public virtual void OnUIHoverExited(UIHoverEventArgs args)
    {
        return;
    }*/
    public virtual void OnClick()
    {
        return;
    }

    // TODO do I even need these two?
    public virtual void OnActivate(ActivateEventArgs args)
    {
        return;
    }
    public virtual void OnDeActivate(DeactivateEventArgs args)
    {
        return;
    }

    // -----------------------------------------  PLAYER INPUTS ----------------------------------------- //
    public virtual void TakeTextureInput(InputAction.CallbackContext context)
    {
        return;
    }
    public virtual void PlaceTextureInput(InputAction.CallbackContext context)
    {
        return;
    }
    public virtual void ActivateGeneration(InputAction.CallbackContext context)
    {
        return;
    }
    public virtual void TakeScreenshot(InputAction.CallbackContext context)
    {
        return;
    }
    public virtual void GeneralActivation(DiffusionTextureChanger dtc)
    {
        return;
    }
}