using System.Collections;
using UnityEngine;

// TODO documentation

public class OpenDoorScript : MonoBehaviour
{
    public Animator doorOpenAnimation;
    
    public AudioSource audioSource;

    private void Start()
    {
        StartCoroutine(OpenDoorTimer());
    }

    public void OpenDoor()
    {
        if (audioSource == null)
        {
            Debug.Log("Add a Audio Source to open door");
            return;
        }

        doorOpenAnimation.Play("Door Open Clip 3");
        audioSource.Play();
    }

    IEnumerator OpenDoorTimer()
    {        
        while(GameManager.getInstance() == null)
        {
            yield return new WaitForSeconds(3f);
        }
        while(audioSource == null)
        {
            yield return new WaitForSeconds(3f);
        }
        OpenDoor();
    }
}
