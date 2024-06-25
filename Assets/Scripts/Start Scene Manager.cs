using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] private TMP_Text display;
    [SerializeField] private TMP_InputField input;
    [SerializeField] private string thisScene, nextScene;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryTransferScene()
    {
        Debug.Log("Called TryTransferScene!");
        if (input.text == "")
        {
            display.text = "IP cannot be empty!";
            return;
        }
        GameManager.getInstance().IP = input.text;
        GameManager.getInstance().LoadNextScene(thisScene, nextScene);
    }
}
