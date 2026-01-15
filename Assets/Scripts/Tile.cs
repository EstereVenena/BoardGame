using UnityEngine;

public class Tile : MonoBehaviour
{
    [Header("Board Index (optional, for debugging)")]
    public int index;

    // Where the pawn should snap (usually the tile pivot)
    public Vector3 SnapPoint => transform.position;
}
