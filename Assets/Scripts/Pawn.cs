using System.Collections;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public int TileIndex { get; private set; } = 0;
    public bool IsMoving { get; private set; }

    [Header("Movement")]
    public float stepTime = 0.18f;
    public float heightOffset = 0.15f;

    [Header("Placement Offset (fix pivot / lane)")]
    public Vector3 tileOffset = Vector3.zero; // set per pawn instance by TurnGameManager

    public void SetTileOffset(Vector3 offset) => tileOffset = offset;

    public void PlaceOnTile(BoardPath board, int index)
    {
        TileIndex = Mathf.Clamp(index, 0, board.LastIndex);
        Vector3 basePos = board.tiles[TileIndex].position;
        transform.position = basePos + tileOffset + Vector3.up * heightOffset;
    }

    public IEnumerator MoveSteps(BoardPath board, int steps)
    {
        if (IsMoving) yield break;
        IsMoving = true;

        for (int i = 0; i < steps; i++)
        {
            if (TileIndex >= board.LastIndex)
                break;

            TileIndex++;
            Vector3 target = board.tiles[TileIndex].position + tileOffset + Vector3.up * heightOffset;
            yield return MoveTo(target, stepTime);
        }

        IsMoving = false;
    }

    IEnumerator MoveTo(Vector3 target, float time)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, time);
            transform.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        transform.position = target;
    }
}
