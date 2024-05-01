using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    private static GameManager instance;

    // Update is called once per frame
    void Update()
    {
        
    }

    public static GameManager getInstance()
    {
        if (instance == null)
        {
            instance = new GameManager();
        }

        return instance;
    }

    public void LoadNextScene(string thisScene, string nextScene)
    {
        SceneManager.UnloadSceneAsync(thisScene);
        SceneManager.LoadScene(nextScene, LoadSceneMode.Additive);
    }
}
