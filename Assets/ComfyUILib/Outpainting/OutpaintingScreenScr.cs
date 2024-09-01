using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Representes a matrix screen used for Outpainting. It is expected that the bottom middle tile, 
/// tile (floor(tileMatrixSize.x/2), tileMatrixSize.y-1) has a starting Texture.
/// </summary>
public class OutpaintingScreenScr : MonoBehaviour
{
    // Size of the tile on the screen
    public Vector3 tileSize = new Vector3(2, 1, 0.01f);

    // The object is cloned and used as the base tile
    public GameObject tileObject;

    // The number of tiles in the matrix on the X/Y axes
    public Vector2Int tileMatrixSize = Vector2Int.one;

    // These represent the first tile that is already textured
    public Vector2Int firstPaintedTile = new Vector2Int(0, 0);
    public Texture2D firstTileTexture;

    // Texture that is used on a tile to represent that it is ready to be painted
    public Texture2D paintableTexture;

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
            if (tile_scr_check == null) return;

            tiles = new GameObject[tileMatrixSize.x, tileMatrixSize.y];

            Vector2Int midTilePos = new Vector2Int(Mathf.CeilToInt(tileMatrixSize.x / 2), 0);

            // TODO make a tile matrix?
            tileObject.transform.localScale = tileSize;
            for (int i = 0; i < tileMatrixSize.x; i++)
            {
                for (int j = 0; j < tileMatrixSize.y; j++)
                {
                    GameObject clone = Instantiate(tileObject, transform.position + new Vector3((((tileMatrixSize.x - 1) / 2) - i) * tileSize.x,
                        (((tileMatrixSize.y - 1) / 2) - j) * tileSize.y, 0), tileObject.transform.rotation);
                    clone.name = "ScreenTile" + i.ToString() + j.ToString();

                    clone.transform.SetParent(GameManager.getInstance().diffusables.transform, false);

                    OutpaintingTile cur_tile_scr = clone.GetComponent<OutpaintingTile>();
                    cur_tile_scr.tilePosition = new Vector2Int(i, j);
                    cur_tile_scr.painted = false;
                    cur_tile_scr.paintable = false;

                    /*if (Mathf.Abs(midTilePos.x - i) == 1 ^ midTilePos.y - j == 1)
                    {
                        cur_tile_scr.paintable = true;                        
                    }*/

                    tiles[i, j] = clone;
                }
            }

            if (firstTileTexture != null)
            {
                Debug.Log("Setting first texture in screen");
                Renderer renderer = tiles[firstPaintedTile.x, firstPaintedTile.y].GetComponent<Renderer>();
                tiles[firstPaintedTile.x, firstPaintedTile.y].GetComponent<OutpaintingTile>().painted = true;
                renderer.material.mainTexture = firstTileTexture;
                renderer.material.SetTexture("_BaseMap", firstTileTexture);
                UpdateTiles(firstPaintedTile);
            }            
        }
    }


    // TODO do I need this function?
    /*public void Paint(Vector2Int tilePos, string keyword)
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
    }*/

    /// <summary>
    /// Helper function for the UpdateTiles function. Checks if a tile is painted, if not, makes it paintable.
    /// </summary>
    /// <param name="tilePos">Position of the tile in the outpainting screen matrix</param>
    private void MakePaintableUnpaintedTile(Vector2Int tilePos)
    {
        OutpaintingTile cur_tile_target = CheckValidTile(tilePos);

        if (cur_tile_target.painted == false)
        {
            cur_tile_target.paintable = true;
            cur_tile_target.GetComponent<Renderer>().material.mainTexture = paintableTexture;
        }
    }

    /// <summary>
    /// Helper function for other functions in this library. Checks if a tilePos corresponds to a real tile and returns the component.
    /// </summary>
    /// <param name="tilePos">Position of the tile in the outpainting screen matrix</param>
    private OutpaintingTile CheckValidTile(Vector2Int tilePos)
    {
        if (tilePos.x > tileMatrixSize.x - 1 || tilePos.x < 0) return null;
        if (tiles[tilePos.x, tilePos.y] == null) return null;

        if (tiles[tilePos.x, tilePos.y].TryGetComponent<OutpaintingTile>(out OutpaintingTile outTile)) return outTile;

        return null;
    }

    /// <summary>
    /// Updates the tiles around the given Tile(given from the position of the tile in the matrix)
    /// </summary>
    /// <param name="tilePos">Position of the tile in the outpainting screen matrix</param>
    public void UpdateTiles(Vector2Int tilePos)
    {
        // Checks if the the tile position is valid
        if (!(tilePos.y < tileMatrixSize.y && tilePos.y >= 0 && tilePos.x < tileMatrixSize.x && tilePos.x >= 0)) return;

        // Paints the current tile and makes it unpaintable beyond that
        OutpaintingTile cur_tile_scr = tiles[tilePos.x, tilePos.y].GetComponent<OutpaintingTile>();
        cur_tile_scr.painted = true;
        cur_tile_scr.paintable = false;

        // We create a difference between tiles in the MIDDLE column and those on the sides.
        // 1. If a middle column tile is painted:
        // it will allow the above tile to be painted,
        // but will not allow the horizontal neighbours to be painted UNLESS they have a painted vertical member beneath
        // 2. If a side column tile is painted:
        // it will allow the horizontal tile neighbours to be painted,
        // but will allow the above tile to be painted IF that top tile has a horizontal painted tile neighbour.

        int midColumnX = (int)Math.Floor((double)tileMatrixSize.x / 2);
        // Checks whether the given Tile is in the middle column
        bool midColumnTileSituation = tilePos.x == midColumnX;
        // True if the tile is on the right of the middle column, False otherwise
        bool tileColumnSide = tilePos.x > midColumnX;

        // TODO REDO THIS HORRIBLE CODE - still pretty bad ngl

        // Makes the above tile paintable
        if (0 < tilePos.y && tilePos.y < tileMatrixSize.y + 1)
        {
            if (midColumnTileSituation)
            {
                MakePaintableUnpaintedTile(new Vector2Int(tilePos.x, tilePos.y - 1));
            }
            else
            {
                if (tileColumnSide)
                {
                    if (CheckValidTile(new Vector2Int(tilePos.x - 1, tilePos.y - 1)).painted) 
                    {
                        MakePaintableUnpaintedTile(new Vector2Int(tilePos.x, tilePos.y - 1));
                    }
                }
                else
                {
                    if (CheckValidTile(new Vector2Int(tilePos.x + 1, tilePos.y - 1)).painted)
                    {
                        MakePaintableUnpaintedTile(new Vector2Int(tilePos.x, tilePos.y - 1));
                    }
                }
            }
        }

        // Makes the left tile paintable
        if (tilePos.x < tileMatrixSize.x - 1)
        {
            if (midColumnTileSituation)
            {
                // This situation is impossible in practicality because:
                // We always choose an image at the beginning to be in the middle lower position.
                if (tilePos.y == tileMatrixSize.y-1)
                {
                    MakePaintableUnpaintedTile(new Vector2Int(tilePos.x-1, tilePos.y));
                }
                else
                {
                    if (CheckValidTile(new Vector2Int(tilePos.x - 1, tilePos.y+1)).painted)
                    {
                        MakePaintableUnpaintedTile(new Vector2Int(tilePos.x-1, tilePos.y));
                    }
                }
            }
            else
            {
                if (tileColumnSide)
                {
                    MakePaintableUnpaintedTile(new Vector2Int(tilePos.x - 1, tilePos.y));
                }
            }

        }

        // Makes the right tile paintable
        if (tilePos.x > 0)
        {
            if (midColumnTileSituation)
            {
                // This situation is impossible in practicality because:
                // We always choose an image at the beginning to be in the middle lower position.
                if (tilePos.y == tileMatrixSize.y - 1)
                {
                    MakePaintableUnpaintedTile(new Vector2Int(tilePos.x + 1, tilePos.y));
                }
                else
                {
                    if (CheckValidTile(new Vector2Int(tilePos.x + 1, tilePos.y + 1)).painted)
                    {
                        MakePaintableUnpaintedTile(new Vector2Int(tilePos.x + 1, tilePos.y));
                    }
                }
            }
            else
            {
                if (!tileColumnSide)
                {
                    MakePaintableUnpaintedTile(new Vector2Int(tilePos.x + 1, tilePos.y));
                }
            }
        }
    }
}
