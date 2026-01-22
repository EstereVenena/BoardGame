using System.Collections;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public int TileIndex { get; private set; }
    public bool IsMoving { get; private set; }

    [Header("Movement")]
    [Tooltip("Seconds per step")]
    [Min(0.01f)] public float stepTime = 0.18f;

    [Tooltip("Height above tile center")]
    public float heightOffset = 0.15f;

    [Tooltip("Use unscaled time (keeps moving even if Time.timeScale = 0). Usually OFF for gameplay.")]
    public bool useUnscaledTime = false;

    [Header("Lane / Pivot Offset")]
    [Tooltip("Small offset from tile center to avoid overlapping pawns. Set per pawn instance.")]
    public Vector3 tileOffset = Vector3.zero;

    [Header("Snap")]
    [Tooltip("After each step, hard-snap to the exact target to avoid drift.")]
    public bool snapAfterStep = true;

    [Tooltip("Tiny extra snap at end of the whole move (safety).")]
    public bool snapAtEnd = true;

    public void SetTileOffset(Vector3 offset) => tileOffset = offset;

    // ---------- Public API ----------

    public void PlaceOnTile(BoardPath board, int index)
    {
        if (!IsBoardValid(board)) return;

        TileIndex = Mathf.Clamp(index, 0, board.LastIndex);
        transform.position = GetTileTarget(board, TileIndex);
    }

    public IEnumerator MoveSteps(BoardPath board, int steps)
    {
        if (IsMoving) yield break;
        if (!IsBoardValid(board)) yield break;

        IsMoving = true;

        // Clamp steps to non-negative
        steps = Mathf.Max(0, steps);

        for (int i = 0; i < steps; i++)
        {
            if (TileIndex >= board.LastIndex) break;

            TileIndex++;

            Vector3 target = GetTileTarget(board, TileIndex);
            yield return MoveTo(target, stepTime);

            if (snapAfterStep)
                transform.position = target; // eliminate any float residue
        }

        if (snapAtEnd)
            transform.position = GetTileTarget(board, TileIndex);

        IsMoving = false;
    }

    // ---------- Internals ----------

    private Vector3 GetTileTarget(BoardPath board, int tileIndex)
    {
        // Tile center (world)
        Vector3 basePos = board.tiles[tileIndex].position;

        // Lane/pivot offset + height above tile
        basePos += tileOffset;
        basePos.y += heightOffset;

        return basePos;
    }

    private IEnumerator MoveTo(Vector3 target, float duration)
    {
        Vector3 start = transform.position;

        // Avoid div by 0 and instant steps
        if (duration <= 0.0001f)
        {
            transform.position = target;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += dt;

            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.LerpUnclamped(start, target, t);

            yield return null;
        }

        transform.position = target;
    }

    private bool IsBoardValid(BoardPath board)
    {
        if (board == null)
        {
            Debug.LogError("[Pawn] BoardPath is NULL.");
            return false;
        }

        if (board.tiles == null || board.tiles.Count == 0 || board.LastIndex < 0)
        {
            Debug.LogError("[Pawn] BoardPath has no tiles.");
            return false;
        }

        return true;
    }
}
