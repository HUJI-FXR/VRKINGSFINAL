using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Represents an object that can create a Diffusion Request.
/// It is not required however that it is part of the diffusables, I.E. it doesn't have to be effected by textures being added, it _could_ just be used to create.
/// </summary>
public class DiffusableObject : MonoBehaviour
{
    // Used to add to the prompts in the DiffusionRequests
    public string keyword;

    // True if it is grabbable, needs to have a GrabInteractable component if that is the case
    public bool grabbable;

    // True if grabbed
    [NonSerialized]
    public bool grabbed;

    // todo maybe the grabbed and ungrabbed should be here as well? maybe another script of theirs instead of gadgetmechanismS?

    private void Start()
    {
        // todo should grabbable raise a bigger alert? do I even need grabbable?
        if (grabbable && GetComponent<XRGrabInteractable>() == null)
        {
            Debug.LogError("Add a GrabInteractable on");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (grabbed)
        {
            return;
        }
        if (GameManager.getInstance().radiusDiffusionTexture == null)
        {
            return;
        }
        GameManager.getInstance().radiusDiffusionTexture.DiffusableObjectCollided(collision);    
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        grabbed = true;
        if (GameManager.getInstance().gadget == null) {
            return;
        }
        GameManager.getInstance().gadget.DiffusableGrabbed(args);
    }
    public void OnSelectExited(SelectExitEventArgs args)
    {
        grabbed = false;
        if (GameManager.getInstance().gadget == null)
        {
            return;
        }
        GameManager.getInstance().gadget.DiffusableUnGrabbed(args);
    }
}
