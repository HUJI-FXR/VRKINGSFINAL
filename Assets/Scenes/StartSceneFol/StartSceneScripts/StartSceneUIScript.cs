using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneUIScript : MonoBehaviour
{
    public TMP_InputField URLInputField;    
    public Button ApplyURLButton;

    public Button StartGameButton;

    private void Start()
    {
        if (ApplyURLButton == null || URLInputField == null || StartGameButton == null)
        {
            Debug.LogError("Please add all the required elements of the Start Scene UI Script");
        }

        StartGameButton.enabled = false;
    }

    public void ApplyURL()
    {
        if (URLInputField.text == "")
        {
            Debug.LogError("Please input a ComfyUI API Server URL");
        }

        StartGameButton.enabled = true;

        // TODO need to add the given URL to the general script or the GameManager if we make it
    }

    // TODO fill the Start Game Button function
    public void StartGame()
    {
        return;
    }
}
