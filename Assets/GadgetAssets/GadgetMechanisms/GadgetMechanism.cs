using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;
using System;

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
    [NonSerialized]
    public string mechanismText;

    public GadgetMechanism()
    {
        this.mechanismText = "";
    }

    /// <summary>
    /// Helper function to make the appropriate DiffusionRequest for a given mechanism.
    /// </summary>
    /// <returns></returns>
    protected virtual DiffusionRequest CreateDiffusionRequest()
    {
        return null;
    }


    /// <summary>
    /// Left hand controller ray hover entered.
    /// </summary>
    public virtual void OnGameObjectLeftHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray hover exited.
    /// </summary>
    public virtual void OnGameObjectLeftHoverExited(HoverExitEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Right hand controller ray hover entered.
    /// </summary>
    public virtual void OnGameObjectRightHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Right hand controller ray hover exited.
    /// </summary>
    public virtual void OnGameObjectRightHoverExited(HoverExitEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray select entered.
    /// </summary>
    public virtual void onGameObjectLeftSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray select exited.
    /// </summary>
    public virtual void onGameObjectLeftSelectExited(SelectExitEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Right hand controller ray select entered.
    /// </summary>
    public virtual void onGameObjectRightSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Right hand controller ray select exited.
    /// </summary>
    public virtual void onGameObjectRightSelectExited(SelectExitEventArgs args)
    {
        return;
    }

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

    /// <summary>
    /// Places a texture on a selected object.
    /// </summary>
    public virtual void PlaceTextureInput(InputAction.CallbackContext context)
    {
        return;
    }

    /// <summary>
    /// Activates the Diffusion image generation.
    /// </summary>
    public virtual void ActivateGeneration(InputAction.CallbackContext context)
    {
        return;
    }

    /// <summary>
    /// Uses a camera to shoot an image.
    /// </summary>
    public virtual void TakeScreenshot(InputAction.CallbackContext context)
    {
        return;
    }
    public virtual void GeneralActivation(DiffusionTextureChanger dtc)
    {
        return;
    }
}