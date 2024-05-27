using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpaintingTile : MonoBehaviour
{
    public Vector2Int tilePosition;
    public bool painted = false;

    void Paint()
    {
        if (painted)
        {
            return;
        }

        // TODO get adjacent tiles
        // TODO get adjacent tile textures
        // TODO make texture to fill up accordingly
        // TODO send texture
        // TODO get outpainted texture for tile
        // TODO add the outpainted texture to tile

        painted = true;
    }

    // TODO cause touch with object on tile to start Paint function
}
