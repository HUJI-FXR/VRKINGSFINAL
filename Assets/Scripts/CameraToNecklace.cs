using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToNecklace : MonoBehaviour
{
    public float timeToMove = 2f;
    public float elapsedTime = 2f;
    public float speed = 0.1f;

    public GameObject socketInteractable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

         // Duration of the movement
        Vector3 startPosition = transform.position;
        Vector3 endPosition = socketInteractable.transform.position;

        transform.position = Vector3.Lerp(startPosition, endPosition, speed);

    }
}
