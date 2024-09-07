using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpaintingTile : MonoBehaviour
{
    public Vector2Int tilePosition;
    public bool painted = false;
    public bool paintable = false;
    public OutpaintingScreenScr out_screen;

    private void OnTriggerEnter(Collider other)
    {
        if (painted || other == null || out_screen == null || GetComponent<RegularDiffusionTexture>() == null || GameManager.getInstance().gadget == null)
        {
            return;
        }

        DiffusableObject diff = other.gameObject.GetComponent<DiffusableObject>();
        if (diff == null) return;
        if (!diff.Model3D) return;
        /*XRGrabInteractable inter = other.gameObject.GetComponent<XRGrabInteractable>();
        if (inter == null) return;*/
        /*Rigidbody rigidbody = inter.gameObject.GetComponent<Rigidbody>();
        if (rigidbody == null) return;*/

        /*rigidbody.useGravity = false;
        out_screen.Paint(tilePosition, diff.keyword);*/

        GameManager.getInstance().gadget.GeneralActivation(GetComponent<RegularDiffusionTexture>());
    }
}
