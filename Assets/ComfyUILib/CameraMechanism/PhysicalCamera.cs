using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Represents a physical virtual Camera, to be used in conjunction with the Camera Gadget Mechanism.
/// </summary>
public class PhysicalCamera : MonoBehaviour
{
    // TODO do I need this bool? I already have CGM.takePicture
    bool screenshotEnabled = true;
    public RenderTexture screenRenderTexture;
    CameraGadgetMechanism CGM = null;

    // The Camera that is attached to this Camera
    // We make it a public variable as it is simple and visual as a whole prefab
    public Camera curCamera;

    private void Start()
    {
        if (screenRenderTexture == null) Debug.LogError("Add a RenderTexture to the Physical Camera, for the display screen");
    }



    // TODO CRITICAL need to check that the CameraGadgetMechanism is ACTIVE and not ONLY that it exists in the Gadget
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


    // TODO descriptions for these:
    /*private void FreezeRenderTexture()
    {
        screenRenderTexture.active = renderTexture;
        frozenTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        frozenTexture.Apply();
        RenderTexture.active = null;
    }*/


    private IEnumerator FreezeShotTimer()
    {
        screenshotEnabled = false;

        // Time until next screenshot is possible
        yield return new WaitForSeconds(1.5f);

        screenshotEnabled = true;
    }

    public void FreezeScreen()
    {
        StartCoroutine(FreezeShotTimer());
    }
}
