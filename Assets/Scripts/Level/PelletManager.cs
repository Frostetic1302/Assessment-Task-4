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
        }

        if (tile == powerPelletTile)
        {
            tilemap.SetTile(cellPosition, dirtTile);

            GameManager.instance.AddScore(50);

            GhostManager.instance.StartScaredMode();
        }
    }
}