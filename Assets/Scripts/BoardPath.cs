using System.Collections.Generic;
using UnityEngine;

public class BoardPath : MonoBehaviour
{
    [Tooltip("Auto-filled from children named Tile0, Tile1, ...")]
    public List<Transform> tiles = new List<Transform>();

    public int LastIndex => tiles.Count - 1;

    void Awake()
    {
        Rebuild();
    }

    [ContextMenu("Rebuild Tiles")]
    public void Rebuild()
    {
        tiles.Clear();

        // Collect all children
        var all = new List<Transform>();
        foreach (Transform child in transform)
            all.Add(child);

        // Sort by number in name: Tile0..Tile19..Tile100
        all.Sort((a, b) => ExtractIndex(a.name).CompareTo(ExtractIndex(b.name)));

        // Keep only Tile* that has a valid number
        foreach (var t in all)
        {
            int idx = ExtractIndex(t.name);
            if (idx >= 0) tiles.Add(t);
        }

        if (tiles.Count == 0)
            Debug.LogError("BoardPath: No tiles found. Name them Tile0, Tile1, ...");
        else
            Debug.Log("BoardPath: Tiles loaded = " + tiles.Count);
    }

    int ExtractIndex(string s)
    {
        // expects "Tile<number>"
        if (!s.StartsWith("Tile")) return -1;
        string num = s.Substring(4);
        int val;
        return int.TryParse(num, out val) ? val : -1;
    }
}
