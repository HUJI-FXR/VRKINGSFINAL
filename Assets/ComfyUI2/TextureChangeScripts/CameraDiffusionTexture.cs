using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CameraDiffusionTexture : DiffusionTextureChanger
{
    new private Queue<List<Texture2D>> diff_Textures = new Queue<List<Texture2D>>();
    private static readonly int MAX_CAMERA_IMAGES = 8;

    public DiffusionQueueStickers stickers;

    private void Start()
    {
        if (stickers == null)
        {
            Debug.LogError("Please add the Diffusion Gadget stickers to the Camera Diffusion Mechanic");
        }
    }

    public override void AddTexture(List<Texture2D> textures, bool addToTextureTotal)
    {
        if (textures == null)
        {
            return;
        }

        if (diff_Textures.Count >= MAX_CAMERA_IMAGES)
        {
            Debug.LogError("Too many images in Gadget Camera Queue");
        }
        diff_Textures.Enqueue(textures);

        if (stickers != null)
        {
            stickers.AddOne();
        }
    }

    public void SendCameraRayInput(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            SendCameraRay();
        }
    }

    public void SendCameraRay()
    {
        if (diff_Textures.Count <= 0)
        {
            Debug.LogError("Tried to add a textures from the Gadget camera without textures in the Queue");
        }

        Transform camera = transform.parent;

        // Perform the raycast
        Ray ray = new Ray(camera.position, camera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit GameObject has the TextureScript component
            DiffusionTextureChanger diffTextureScript = hit.collider.GetComponent<DiffusionTextureChanger>();
            if (diffTextureScript != null)
            {
                diffTextureScript.AddTexture(diff_Textures.Dequeue(), false);

                if (stickers != null)
                {
                    stickers.RemoveOne();
                }
            }
        }
    }
}
