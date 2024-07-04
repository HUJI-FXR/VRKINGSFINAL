using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutpaintingScreenScr : MonoBehaviour
{
    public Vector3 tileSize = new Vector3(2, 1, 0.01f);
    public GameObject tileObject;
    public Vector2Int tileMatrixSize = Vector2Int.one;    
    
    private GameObject[,] tiles;

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
            
            tiles = new GameObject[tileMatrixSize.x, tileMatrixSize.y];

            // TODO make a tile matrix?
            tileObject.transform.localScale = tileSize;
            for (int i = 0; i < tileMatrixSize.x; i++)
            {
                for (int j = 0; j < tileMatrixSize.y; j++)
                {
                    GameObject clone = Instantiate(tileObject, transform.position + new Vector3((((tileMatrixSize.x-1)/2) - i)*tileSize.x, 
                        (((tileMatrixSize.y - 1) / 2) - j) * tileSize.y, 0), transform.rotation);

                    if (GameManager.getInstance().diffusables == null)
                    {
                        Debug.Log("DIFFUSABLES DONT EXIST");
                    }

                    clone.transform.SetParent(GameManager.getInstance().diffusables.transform, false);

                    OutpaintingTile cur_tile_scr = clone.GetComponent<OutpaintingTile>();                    
                    cur_tile_scr.tilePosition = new Vector2Int(i, j);
                    cur_tile_scr.painted = false;

                    tiles[i, j] = clone;
                }
            }
        }
    }


    public void Paint(Vector2Int tilePos, string keyword)
    {
        // TODO assuming that we are not yet taking into accoount a situation where we generate a middle image between 8 generated tiles
        // TODO get adjacent tiles - currently only taking into account the left tile
        if (tilePos.x == 0)
        {
            return;
        }
        if (tilePos.x > 0)
        {
            if (!tiles[tilePos.x-1, tilePos.y].GetComponent<OutpaintingTile>().painted)
            {
                return;
            }
        }

        // TODO get adjacent tile textures - currently only taking into account the left tile
        Renderer tileRenderer = tiles[tilePos.x-1, tilePos.y].GetComponent<Renderer>();
        Texture prevTexture = tileRenderer.material.GetTexture("_MainTex");

        // TODO make texture to fill up accordingly
        // TODO send texture
        // TODO get outpainted texture for tile
        // TODO add the outpainted texture to tile

        tiles[tilePos.x, tilePos.y].GetComponent<OutpaintingTile>().painted = true;
    }
}
