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

    // TODO implement in the mechanisms - NOTE: should this be implemented in the throwing mechanism? ALSO what happens with uiDiffusionTexture?
    /// <summary>
    /// Resets the Mechanism, removing everything it has selected.
    /// </summary>
    protected virtual void ResetMechanism()
    {
        return;
    }

    /// <summary>
    /// Left hand controller ray hover entered.
    /// </summary>
    public virtual void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray hover exited.
    /// </summary>
    public virtual void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray select entered.
    /// </summary>
    public virtual void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    /// <summary>
    /// Left hand controller ray select exited.
    /// </summary>
    public virtual void onGameObjectSelectExited(SelectExitEventArgs args)
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

    /// <summary>
    /// Places a texture on a selected object.
    /// </summary>
    public virtual void PlaceTextureInput(GameObject GO)
    {
        return;
    }

    /// <summary>
    /// Activates the Diffusion image generation.
    /// </summary>
    public virtual void ActivateGeneration(GameObject GO)
    {
        return;
    }

    // TODO might not need this one either
    /// <summary>
    /// Uses a camera to shoot an image.
    /// </summary>
    public virtual void TakeScreenshot(Camera camera)
    {
        return;
    }
    public virtual void GeneralActivation(DiffusionTextureChanger dtc)
    {
        return;
    }
}