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

        Vector3Int cellPosition =
            tilemap.WorldToCell(worldPosition);

        TileBase tile =
            tilemap.GetTile(cellPosition);

        if (tile == null)
        {
            return false;
        }

        return wallTiles.Contains(tile);
    }
}