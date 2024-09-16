using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraToNecklace : MonoBehaviour
{

    public float speed = 0.05f;

    public float delay = 1.0f;

    public bool returning = false;

    public GameObject socketInteractable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (returning)
        {
            // Duration of the movement
            Vector3 startPosition = transform.position;
            Vector3 endPosition = socketInteractable.transform.position;

            transform.position = Vector3.Lerp(startPosition, endPosition, speed);
        }

    }

    public void SummonBack()
    {
        StartCoroutine(DragToSocket());
    }

    public void StopSummoning()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.useGravity = true;
        }

        returning = false;
    }

    IEnumerator DragToSocket()
    {
        yield return new WaitForSeconds(delay);

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null )
        {
            body.useGravity = false;
        }

        returning = true;

        yield return null;

    }
}
