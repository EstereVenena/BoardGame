using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Assign the parent Transform that contains tile GameObjects (with Tile.cs)")]
    public Transform tilesParent;

    [Header("Optional: auto-fill tiles from parent on Start")]
    public bool autoCollectTiles = true;

    [SerializeField] private List<Tile> tiles = new List<Tile>();

    public int TileCount => tiles.Count;

    private void Awake()
    {
        if (autoCollectTiles)
        {
            CollectTilesFromParent();
        }

        // Give tiles indexes (handy for debugging)
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i] != null) tiles[i].index = i;
        }
    }

    public void CollectTilesFromParent()
    {
        tiles.Clear();

        if (tilesParent == null)
        {
            Debug.LogError("[BoardManager] tilesParent is not assigned.");
            return;
        }

        Tile[] found = tilesParent.GetComponentsInChildren<Tile>(true);
        tiles.AddRange(found);

        if (tiles.Count == 0)
            Debug.LogError("[BoardManager] No Tile components found under tilesParent.");
    }

    public Tile GetTile(int index)
    {
        if (tiles.Count == 0) return null;

        index = Mathf.Clamp(index, 0, tiles.Count - 1);
        return tiles[index];
    }

    // For boards where you loop back to start (like many board games)
    public int WrapIndex(int index)
    {
        if (tiles.Count == 0) return 0;
        index %= tiles.Count;
        if (index < 0) index += tiles.Count;
        return index;
    }

    // For boards that stop at last tile (no looping)
    public int ClampIndex(int index)
    {
        if (tiles.Count == 0) return 0;
        return Mathf.Clamp(index, 0, tiles.Count - 1);
    }
}
