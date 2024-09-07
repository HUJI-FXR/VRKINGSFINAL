using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitSceneChangeTrigger : MonoBehaviour
{ 
    public string currentRoomSceneName = "TileRoom";
    public string nextRoomSceneName = "Camera Full Prototype Scene";
    public string altNextRoomSceneName = "GadgetPrefabTest";

    public Toggle toggle;

    private void OnTriggerEnter(Collider other)
    {
        ChangeScene();
    }

    public void ChangeScene(TMP_InputField txt)
    {
        if (GameManager.getInstance() == null) return;

        GameManager.getInstance().IP = txt.text;
        if (toggle.isOn)
        {
            GameManager.getInstance().LoadNextScene(currentRoomSceneName, nextRoomSceneName);
        }   
        else
        {
            GameManager.getInstance().LoadNextScene(currentRoomSceneName, altNextRoomSceneName);
        }
    }

    public void ChangeScene()
    {
        GameManager.getInstance().LoadNextScene(currentRoomSceneName, nextRoomSceneName);
    }
}
