using System.Collections;
using UnityEngine;

public class OpenDoorScript : MonoBehaviour
{
    public Animation doorOpenAnimation;
    public AudioClip doorOpenAudioClip;

    private void Start()
    {
        StartCoroutine(OpenDoorTimer());
    }

    public void OpenDoor()
    {
        doorOpenAnimation.Play();
        GameManager.getInstance().headAudioSource.PlayOneShot(doorOpenAudioClip);
    }

    IEnumerator OpenDoorTimer()
    {        
        while(GameManager.getInstance() == null)
        {
            yield return new WaitForSeconds(3f);
        }
        while(GameManager.getInstance().headAudioSource == null)
        {
            yield return new WaitForSeconds(3f);
        }
        OpenDoor();
    }
}
