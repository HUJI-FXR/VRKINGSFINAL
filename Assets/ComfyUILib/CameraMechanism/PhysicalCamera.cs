using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Represents a physical virtual Camera, to be used in conjunction with the Camera Gadget Mechanism.
/// </summary>
public class PhysicalCamera : MonoBehaviour
{    
    CameraGadgetMechanism CGM = null;

    // The Camera that is attached to this Camera
    // We make it a public variable as it is simple and visual as a whole prefab
    public Camera curCamera;

    /// <summary>
    /// Helper function which retreives the relevant CameraGadgetMechanism
    /// </summary>
    /// <returns>True if succeeded in getting the CameraGadgetMechanism</returns>
    private bool GetCameraGadgetMechanism()
    {
        if (CGM == null)
        {
            foreach (GadgetMechanism GM in GameManager.getInstance().gadget.GadgetMechanisms)
            {
                if (GM is CameraGadgetMechanism)
                {
                    CGM = (CameraGadgetMechanism)GM;
                }
            }
            if (CGM == null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Called when the Physical Camera is grabbed, allows for screenshotting
    /// </summary>
    /// <param name="args"></param>
    public void CameraGrabbed(SelectEnterEventArgs args)
    {
        if (!GetCameraGadgetMechanism()) return;
        CGM.takePicture = true;
        Debug.Log("Take pic " + CGM.takePicture.ToString());
    }

    /// <summary>
    /// Called when the Physical Camera is ungrabbed, removes ability to screenshot
    /// </summary>
    /// <param name="args"></param>
    public void CameraUnGrabbed(SelectExitEventArgs args)
    {
        if (!GetCameraGadgetMechanism()) return;
        CGM.takePicture = false;
    }

    /// <summary>
    /// Sends a screenshot request    
    /// </summary>
    /// <param name="args"></param>
    public void CameraScreenshot(ActivateEventArgs args)
    {
        if (!GetCameraGadgetMechanism() || curCamera == null) return;
        GameManager.getInstance().gadget.TakeScreenshot(curCamera);
    }
}
