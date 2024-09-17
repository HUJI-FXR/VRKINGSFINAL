using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpaintingTile : MonoBehaviour
{
    public Vector2Int tilePosition;
    public bool painted = false;
    public bool paintable = false;
    public OutpaintingScreenScr out_screen;

    // TODO do I even need this?
    /*private void OnTriggerEnter(Collider other)
    {
        if (painted || other == null || out_screen == null || 
            GetComponent<RegularDiffusionTexture>() == null || GameManager.getInstance().gadget == null)
        {
            return;
        }

        DiffusableObject diff = other.gameObject.GetComponent<DiffusableObject>();
        if (diff == null) return;
        if (!diff.Model3D) return;

        GameManager.getInstance().gadget.GeneralActivation(GetComponent<RegularDiffusionTexture>());
    }*/

    /// <summary>
    /// Used to indicate to the tile that it has been painted
    /// </summary>
    /// <param name="curPaintedStatus">Final painting status</param>
    public void SetPainted(bool curPaintedStatus)
    {
        painted = curPaintedStatus;
        out_screen.UpdateTiles(tilePosition);
    }
}
