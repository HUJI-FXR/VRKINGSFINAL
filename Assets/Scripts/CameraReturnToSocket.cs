
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraReturnToSocket : MonoBehaviour
{
    [SerializeField] private XRSocketInteractor socketInteractor; // Reference to your XRSocketInteractor
    [SerializeField] private Transform socketTransform; // The position where the camera should be returned
    [SerializeField] private float returnSpeed = 5f; // Speed at which the camera will return to the socket

    private Transform cameraTransform;

    private void Start()
    {
        if (socketInteractor == null)
        {
            Debug.LogError("Socket Interactor is not assigned.");
            return;
        }

        if (socketTransform == null)
        {
            Debug.LogError("Socket Transform is not assigned.");
            return;
        }

        // Add a listener to the onSelectExited event        
        socketInteractor.onSelectExited.AddListener(HandleCameraRelease);
    }

    private void OnDestroy()
    {
        // Clean up the event listener
        if (socketInteractor != null)
        {
            socketInteractor.onSelectExited.RemoveListener(HandleCameraRelease);
        }
    }

    private void HandleCameraRelease(XRBaseInteractable interactable)
    {
        if (interactable.transform == cameraTransform)
        {
            // Start the coroutine to return the camera to the socket
            StartCoroutine(ReturnCameraToSocket());
        }
    }

    private System.Collections.IEnumerator ReturnCameraToSocket()
    {
        float elapsedTime = 0f;
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        while (elapsedTime < 1f)
        {
            cameraTransform.position = Vector3.Lerp(startPosition, socketTransform.position, elapsedTime);
            cameraTransform.rotation = Quaternion.Slerp(startRotation, socketTransform.rotation, elapsedTime);

            elapsedTime += Time.deltaTime * returnSpeed;
            yield return null;
        }

        // Ensure the final position and rotation are exactly at the socket
        cameraTransform.position = socketTransform.position;
        cameraTransform.rotation = socketTransform.rotation;
    }

    // You might want to set the cameraTransform manually or find it dynamically
    public void SetCameraTransform(Transform transform)
    {
        cameraTransform = transform;
    }
}
