using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiffusionInput : MonoBehaviour
{
    private string[] diffusionMechanics = { "Create Image", "Combine Images", "Camera to Image", "Depth Camera to Image" };
    private int curMechanicIndex = 0;

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
