using UnityEngine;

public class PawnController : MonoBehaviour
{
    [Header("References")]
    public BoardManager board;

    [Header("Pawn Positioning")]
    public Vector3 pawnOffset = new Vector3(0.2f, 0.2f, 0f);
    public bool keepZFixed = true;
    public float fixedZ = 0f;

    [Header("Current Tile")]
    public int currentTileIndex = 0;

    [Header("Board Behavior")]
    public bool loopAtEnd = true; // true = wrap around, false = clamp at last tile

    private void Awake()
    {
        // If not assigned in Inspector, auto-find one in the scene.
        if (board == null)
            board = FindAnyObjectByType<BoardManager>(); // NEW API (replaces FindObjectOfType)

        if (board == null)
            Debug.LogError("[PawnController] No BoardManager found in scene.");
    }

    private void Start()
    {
        SnapToCurrentTile();
    }

    public void SnapToCurrentTile()
    {
        if (board == null)
        {
            Debug.LogError("[PawnController] BoardManager reference missing.");
            return;
        }

        Tile tile = board.GetTile(currentTileIndex);
        if (tile == null)
        {
            Debug.LogError("[PawnController] Tile not found for index: " + currentTileIndex);
            return;
        }

        Vector3 target = tile.SnapPoint + pawnOffset;

        if (keepZFixed)
            target.z = fixedZ;

        transform.position = target;
    }

    public void MoveSteps(int steps)
    {
        if (board == null || board.TileCount == 0) return;

        int nextIndex = currentTileIndex + steps;

        currentTileIndex = loopAtEnd
            ? board.WrapIndex(nextIndex)
            : board.ClampIndex(nextIndex);

        SnapToCurrentTile();
    }

    public void MoveToTile(int index)
    {
        if (board == null || board.TileCount == 0) return;

        currentTileIndex = loopAtEnd
            ? board.WrapIndex(index)
            : board.ClampIndex(index);

        SnapToCurrentTile();
    }
}
