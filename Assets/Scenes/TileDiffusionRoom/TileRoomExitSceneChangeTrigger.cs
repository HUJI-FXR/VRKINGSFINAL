using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitSceneChangeTrigger : MonoBehaviour
{ 
    public string currentRoomSceneName = "TileRoom";
    public string nextRoomSceneName = "Camera Full Prototype Scene";
    private void OnCollisionEnter(Collision collision)
    {
        GameManager.getInstance().LoadNextScene(currentRoomSceneName, nextRoomSceneName);
    }
}
