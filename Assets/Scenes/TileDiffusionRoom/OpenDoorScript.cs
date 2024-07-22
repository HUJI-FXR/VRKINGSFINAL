using UnityEngine;

public class OpenDoorScript : MonoBehaviour
{
    public Animation doorOpenAnimation;
    public AudioClip doorOpenAudioClip;

    private void Start()
    {
        OpenDoor();
    }

    public void OpenDoor()
    {
        doorOpenAnimation.Play();
        GameManager.getInstance().headAudioSource.PlayOneShot(doorOpenAudioClip);
    }
}
