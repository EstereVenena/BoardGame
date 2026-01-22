using UnityEngine;

public class PawnController : MonoBehaviour
{
    [Header("References")]
    public BoardManager board;

    [Header("Pawn Positioning")]
    public Vector3 pawnOffset = new Vector3(0.2f, 0f, 0f);

    [Tooltip("Keeps pawn at a fixed height above the tile surface")]
    public bool keepYFixed = true;

    [Tooltip("Extra height added on top of the tile SnapPoint Y")]
    public float yOffset = 0.1f;

    [Header("Current Tile")]
    public int currentTileIndex = 0;

    [Header("Board Behavior")]
    public bool loopAtEnd = true;

    private void Awake()
    {
        if (board == null)
            board = FindAnyObjectByType<BoardManager>();

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

        // IMPORTANT: do NOT overwrite Z (board is on X/Z plane)
        Vector3 target = tile.SnapPoint + pawnOffset;

        if (keepYFixed)
            target.y = tile.SnapPoint.y + yOffset;  // keep pawn above tile surface

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
