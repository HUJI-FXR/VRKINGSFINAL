using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Represents a physical virtual Camera, to be used in conjunction with the Camera Gadget Mechanism.
/// </summary>
public class PhysicalCamera : MonoBehaviour
{
    bool timerActivated = false;

    public RenderTexture screenRenderTexture;
    public GameObject screenPlane;

    // The Camera that is attached to this Camera
    // We make it a public variable as it is simple and visual as a whole prefab
    public Camera curCamera;

    private void Start()
    {
        if (screenRenderTexture == null || screenPlane == null) Debug.LogError("Add the requirements for the physical camera");
    }

    /// <summary>
    /// Sends a screenshot request    
    /// </summary>
    /// <param name="args"></param>
    public void CameraScreenshot(ActivateEventArgs args)
    {        
        if (curCamera == null || timerActivated) return;

        Texture2D screenShot = GeneralGameLibraries.TextureManipulationLibrary.CaptureScreenshot(curCamera);
        GameManager.getInstance().gadget.TakeScreenshot(screenShot, curCamera);
        StartCoroutine(FreezeShotTimer(screenShot));
    }

    /// <summary>
    /// Freezes the Physical Camera screen with the received screenshot, 
    /// and unfreezes the screen when it is allowed to take the next screenshot
    /// </summary>
    /// <param name="screenShot">Received screenshot to freeze screen on</param>    
    private IEnumerator FreezeShotTimer(Texture2D screenShot)
    {
        // Time until next screenshot is possible
        const float timeToWait = 2.0f;

        timerActivated = true;

        // Time waiting for screenshot to render
        yield return new WaitForSeconds(1.0f);

        // Freezing screen
        screenPlane.GetComponent<Renderer>().material.SetTexture("_BaseMap", screenShot);

        curCamera.targetTexture = screenRenderTexture;       
        yield return new WaitForSeconds(timeToWait);

        // Unfreezing screen        
        screenPlane.GetComponent<Renderer>().material.SetTexture("_BaseMap", screenRenderTexture);

        timerActivated = false;

        yield break;
    }
}
