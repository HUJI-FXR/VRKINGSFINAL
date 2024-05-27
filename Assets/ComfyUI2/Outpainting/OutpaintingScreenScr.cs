using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpaintingScreenScr : MonoBehaviour
{
    public Vector3 tileSize = new Vector3(2, 1, 0.01f);
    public GameObject tileObject;
    public Vector2Int tileMatrixSize = Vector2Int.one;

    private void OnValidate()
    {
        // tileMatrixSize needs to be positive
        if (tileMatrixSize.x <= 0)
        {
            tileMatrixSize.x = 1;
        }
        if (tileMatrixSize.y <= 0)
        {
            tileMatrixSize.y = 1;
        }
        // tileMatrixSize needs to be odd
        if (tileMatrixSize.x % 2 == 0)
        {
            tileMatrixSize.x -= 1;
        }
        if (tileMatrixSize.y % 2 == 0)
        {
            tileMatrixSize.y -= 1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (tileObject != null)
        {
            OutpaintingTile tile_scr_check = tileObject.GetComponent<OutpaintingTile>();
            if (tile_scr_check == null)
            {
                return;
            }
            
            // TODO make a tile matrix?
            tileObject.transform.localScale = tileSize;
            for (int i = 0; i < tileMatrixSize.x; i++)
            {
                for (int j = 0; j < tileMatrixSize.y; j++)
                {
                    GameObject clone = Instantiate(tileObject, transform.position + new Vector3((((tileMatrixSize.x-1)/2) - i)*tileSize.x, 
                        (((tileMatrixSize.y - 1) / 2) - j) * tileSize.y, 0), transform.rotation);

                    OutpaintingTile cur_tile_scr = clone.GetComponent<OutpaintingTile>();
                    cur_tile_scr.tilePosition = new Vector2Int(i, j);
                    cur_tile_scr.painted = false;
                }
            }
        }
    }
}
