using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using static GeneralGameLibraries;
using UnityEngine.InputSystem;


// TODO need to remove button from several of these, choose exactly
public class GadgetMechanism : MonoBehaviour
{
    public static string MECHANISM_PRETEXT = "Mechanism:\n";
    
    public string mechanismText;
    public string buttonText;
    public GadgetMechanism()
    {
        this.mechanismText = "";
        this.buttonText = "";
    }

    public virtual void OnGameObjectHoverEntered(HoverEnterEventArgs args)
    {
        return;
    }
    public virtual void OnGameObjectHoverExited(HoverExitEventArgs args)
    {
        return;
    }
    public virtual void onGameObjectSelectEntered(SelectEnterEventArgs args)
    {
        return;
    }
    public virtual void onGameObjectSelectExited(SelectExitEventArgs args)
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
}