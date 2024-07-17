using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OutpaintingScreenScr : MonoBehaviour
{
    public Vector3 tileSize = new Vector3(2, 1, 0.01f);
    public GameObject tileObject;
    public Vector2Int tileMatrixSize = Vector2Int.one;
    public Vector2Int firstPaintedTile = new Vector2Int(0, 0);
    public Texture2D firstTileTexture;

    [NonSerialized]
    public GameObject[,] tiles;

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

    /// <summary>
    /// Creates a screen of OutpaintingTiles of size tileMatrixSize with tiles of size tileSize and adds these to the tiles
    /// </summary>
    public void CreateScreen()
    {
        if (tileObject != null)
        {
            OutpaintingTile tile_scr_check = tileObject.GetComponent<OutpaintingTile>();
            if (tile_scr_check == null)
            {
                return;
            }

            tiles = new GameObject[tileMatrixSize.x, tileMatrixSize.y];

            Vector2Int midTilePos = new Vector2Int(Mathf.CeilToInt(tileMatrixSize.x / 2), 0);

            // TODO make a tile matrix?
            tileObject.transform.localScale = tileSize;
            for (int i = 0; i < tileMatrixSize.x; i++)
            {
                for (int j = 0; j < tileMatrixSize.y; j++)
                {
                    GameObject clone = Instantiate(tileObject, transform.position + new Vector3((((tileMatrixSize.x - 1) / 2) - i) * tileSize.x,
                        (((tileMatrixSize.y - 1) / 2) - j) * tileSize.y, 0), transform.rotation);

                    clone.transform.SetParent(GameManager.getInstance().diffusables.transform, false);

                    OutpaintingTile cur_tile_scr = clone.GetComponent<OutpaintingTile>();
                    cur_tile_scr.tilePosition = new Vector2Int(i, j);
                    cur_tile_scr.painted = false;
                    cur_tile_scr.paintable = false;

                    if (Mathf.Abs(midTilePos.x - i) == 1 ^ midTilePos.y - j == 1)
                    {
                        cur_tile_scr.paintable = true;                        
                    }

                    tiles[i, j] = clone;
                }
            }

            if (firstTileTexture != null)
            {
                Debug.Log("Setting first texture in screen");
                Renderer renderer = tiles[firstPaintedTile.x, firstPaintedTile.y].GetComponent<Renderer>();
                renderer.material.mainTexture = firstTileTexture;
                renderer.material.SetTexture("_BaseMap", firstTileTexture);
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

    /// <summary>
    /// Updates the tiles around the given Tile(given from the position of the tile in the matrix)
    /// </summary>
    /// <param name="tilePos">Position of the tile in the outpainting screen matrix</param>
    private void UpdateTiles(Vector2Int tilePos)
    {
        // Checks if the the tile position is valid
        if (!(tilePos.y < tileMatrixSize.y && tilePos.y >= 0 && tilePos.x < tileMatrixSize.x && tilePos.x >= 0))
        {
            return;
        }

        // Paints the current tile and makes it unpaintable beyond that
        OutpaintingTile cur_tile_scr = tiles[tilePos.x, tilePos.y].GetComponent<OutpaintingTile>();
        cur_tile_scr.painted = true;
        cur_tile_scr.paintable = false;

        // Makes the above tile paintable
        if (tilePos.y < tileMatrixSize.y + 1)
        {
            OutpaintingTile cur_tile_target = tiles[tilePos.x, tilePos.y-1].GetComponent<OutpaintingTile>();
            if (cur_tile_target.painted == false)
            {
                cur_tile_target.paintable = true;
            }            
        }

        // Makes the right tile paintable
        if (tilePos.x < tileMatrixSize.x - 1)
        {
            OutpaintingTile cur_tile_target = tiles[tilePos.x+1, tilePos.y].GetComponent<OutpaintingTile>();
            if (cur_tile_target.painted == false)
            {
                cur_tile_target.paintable = true;
            }
        }

        // Makes the left tile paintable
        if (tilePos.x > 0)
        {
            OutpaintingTile cur_tile_target = tiles[tilePos.x-1, tilePos.y].GetComponent<OutpaintingTile>();
            if (cur_tile_target.painted == false)
            {
                cur_tile_target.paintable = true;
            }
        }
    }
}
