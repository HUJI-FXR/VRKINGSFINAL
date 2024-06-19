using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DiffusableObject : MonoBehaviour
{
    public string keyword;
    public bool grabbable;

    [NonSerialized]
    public bool grabbed;
    /*[NonSerialized]
    private bool allowCollision;*/
    //public DiffusionRequest diffusionRequest;

    // todo make gamemanager script

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
        GeneralGameScript.instance.radiusDiffusionTexture.DiffusableObjectCollided(collision);
        //allowCollision = false;
        
    }

    public void OnSelectEntered(SelectEnterEventArgs args)
    {
        grabbed = true;
        if (GeneralGameScript.instance.gadget == null) {
            return;
        }
        GeneralGameScript.instance.gadget.DiffusableGrabbed(args);
        //allowCollision = true;
    }
    public void OnSelectExited(SelectExitEventArgs args)
    {
        grabbed = false;
        if (GeneralGameScript.instance.gadget == null)
        {
            return;
        }
        GeneralGameScript.instance.gadget.DiffusableUnGrabbed(args);
    }
}
