using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallManager : MonoBehaviour
{
    public static WallManager instance;
    public Tilemap topLeft;
    public Tilemap topRight;
    public Tilemap bottomLeft;
    public Tilemap bottomRight;
    public List<TileBase> wallTiles;
    public TileBase ghostWallTile;
    public List<TileBase> outerWallTiles;

    void Awake()
    {
        instance = this;
    }

    public bool IsWall(Vector3 worldPosition)
    {
        if (CheckTilemap(topLeft, worldPosition)) return true;
        if (CheckTilemap(topRight, worldPosition)) return true;
        if (CheckTilemap(bottomLeft, worldPosition)) return true;
        if (CheckTilemap(bottomRight, worldPosition)) return true;

        return false;
    }

    bool CheckTilemap(Tilemap tilemap, Vector3 worldPosition)
    {
        if (tilemap == null)
        {
            return false;
        }

        Vector3 snappedPosition = new Vector3(Mathf.Round(worldPosition.x - 0.5f) + 0.5f, Mathf.Round(worldPosition.y - 0.5f) + 0.5f, worldPosition.z);
        Vector3Int cellPosition = tilemap.WorldToCell(snappedPosition);
        TileBase tile = tilemap.GetTile(cellPosition);

        if (tile == null)
        {
            return false;
        }
        return wallTiles.Contains(tile);
    }

    public bool IsGhostWall(Vector3 worldPosition)
    {
        if (CheckGhostWall(topLeft, worldPosition)) return true;
        if (CheckGhostWall(topRight, worldPosition)) return true;
        if (CheckGhostWall(bottomLeft, worldPosition)) return true;
        if (CheckGhostWall(bottomRight, worldPosition)) return true;

        return false;
    }

    bool CheckGhostWall(Tilemap tilemap, Vector3 worldPosition)
    {
        if (tilemap == null)
        {
            return false;
        }

        Vector3 snappedPosition = new Vector3(Mathf.Round(worldPosition.x - 0.5f) + 0.5f, Mathf.Round(worldPosition.y - 0.5f) + 0.5f, worldPosition.z);
        Vector3Int cellPosition = tilemap.WorldToCell(snappedPosition);
        TileBase tile = tilemap.GetTile(cellPosition);
        return tile == ghostWallTile;
    }

    public bool IsOuterWall(Vector3 worldPosition)
    {
        return CheckOuterWall(topLeft, worldPosition) || CheckOuterWall(topRight, worldPosition) || CheckOuterWall(bottomLeft, worldPosition) || CheckOuterWall(bottomRight, worldPosition);
    }

    bool CheckOuterWall(Tilemap tilemap, Vector3 worldPosition)
    {
        if (tilemap == null)
        {
            return false;
        }
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        TileBase tile = tilemap.GetTile(cellPosition);

        if (tile == null)
        {
            return false;
        }
        return outerWallTiles.Contains(tile);
    }

}