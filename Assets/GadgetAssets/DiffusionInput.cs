using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiffusionInput : MonoBehaviour
{
    private string[] diffusionMechanics = { "Create Image", "Combine Images", "Camera to Image", "Depth Camera to Image" };
    private int curMechanicIndex = 0;

    public CameraDiffusionTexture cameraDiffTexture;

    [SerializeField]
    private InputActionAsset inputActions;
    private InputAction takePicture;

    private void Start()
    {
        /*var diffusionActionMap = inputActions.FindActionMap("DiffusionMechanicPicker");
        diffusionActionMap.Enable();
        diffusionActionMap.actionTriggered += cameraDiffTexture.SendCameraRayInput;*/

        /*takePicture = diffusionActionMap.FindAction("TakePicture");
        takePicture.Enable();
        takePicture.actionTriggered += cameraDiffTexture.SendCameraRayInput;*/
    }

    public void ChangeToNextMechanic()
    {
        curMechanicIndex++;
        curMechanicIndex %= diffusionMechanics.Length;
        ChangeToMechanic(curMechanicIndex);
    }
    public void ChangeToPreviousMechanic()
    {
        curMechanicIndex--;
        curMechanicIndex %= diffusionMechanics.Length;
        ChangeToMechanic(curMechanicIndex);
    }

    public void ChangeToMechanic(int index)
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().text = diffusionMechanics[index];
    }
}
