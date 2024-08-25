using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitSceneChangeTrigger : MonoBehaviour
{ 
    public string currentRoomSceneName = "TileRoom";
    public string nextRoomSceneName = "Camera Full Prototype Scene";
    private void OnTriggerEnter(Collider other)
    {
        GameManager.getInstance().LoadNextScene(currentRoomSceneName, nextRoomSceneName);
    }

    public void ChangeScene(TMP_InputField txt)
    {
        GameManager.getInstance().IP = txt.text;
        GameManager.getInstance().LoadNextScene(currentRoomSceneName, nextRoomSceneName);
    }
}
