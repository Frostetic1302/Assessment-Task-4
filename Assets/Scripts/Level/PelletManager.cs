using UnityEngine;
using UnityEngine.Tilemaps;

public class PelletManager : MonoBehaviour
{
    public Tilemap topLeft;
    public Tilemap topRight;
    public Tilemap bottomLeft;
    public Tilemap bottomRight;
    public TileBase pelletTile;
    public TileBase dirtTile;
    public TileBase powerPelletTile;
    private int totalPellets;

    void Start()
    {
        totalPellets += CountPelletsInTilemap(topLeft);
        totalPellets += CountPelletsInTilemap(topRight);
        totalPellets += CountPelletsInTilemap(bottomLeft);
        totalPellets += CountPelletsInTilemap(bottomRight);

        int CountPelletsInTilemap(Tilemap tilemap)
        {
            if (tilemap == null) return 0;
            int count = 0;
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = tilemap.GetTile(pos);
                if (tile == pelletTile || tile == powerPelletTile)
                {
                    count++;
                }
            }
            return count;
        }
    }

    public void EatPellet(Vector3 worldPosition)
    {
        ReplaceTile(topLeft, worldPosition);
        ReplaceTile(topRight, worldPosition);
        ReplaceTile(bottomLeft, worldPosition);
        ReplaceTile(bottomRight, worldPosition);
    }

    void ReplaceTile(Tilemap tilemap, Vector3 worldPosition)
    {
        Vector3Int cellPosition =
            tilemap.WorldToCell(worldPosition);

        TileBase tile =
            tilemap.GetTile(cellPosition);

        if (tile == pelletTile)
        {
            tilemap.SetTile(cellPosition, dirtTile);
            GameManager.instance.AddScore(10);
            OnPelletEaten();
        }

        if (tile == powerPelletTile)
        {
            tilemap.SetTile(cellPosition, dirtTile);
            GameManager.instance.AddScore(50);
            GhostManager.instance.StartScaredMode();
            OnPelletEaten();
        }
    }
    void OnPelletEaten()
    {
        totalPellets--;

        if (totalPellets <= 0)
        {
            GameManager.instance.GameOver();
        }
    }
}