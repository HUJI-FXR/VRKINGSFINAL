using System.Collections;
using UnityEngine;

public class OpenDoorScript : MonoBehaviour
{
    public Animation doorOpenAnimation;
    public AudioClip doorOpenAudioClip;
    
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

        doorOpenAnimation.Play();
        audioSource.PlayOneShot(doorOpenAudioClip);
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
