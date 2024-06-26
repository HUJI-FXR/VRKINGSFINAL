using System.Collections;
using System.Collections.Generic;
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
}
