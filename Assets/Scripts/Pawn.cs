using System.Collections;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public int TileIndex { get; private set; } = 0;
    public bool IsMoving { get; private set; }

    [Header("Movement")]
    public float stepTime = 0.18f;
    public float heightOffset = 0.15f;

    /* ===============================
     * ANIMATION (DISABLED FOR NOW)
     * ===============================
     * To enable later:
     * 1. Uncomment Animator field
     * 2. Uncomment SetRun(...) calls
     * 3. Assign Animator in prefab
     */

    /*
    [Header("Animation (optional)")]
    public Animator animator;
    public string idleParam = "Idle";
    public string runParam = "Run";
    */

    void Awake()
    {
        // If you want auto-detection later:
        // if (animator == null)
        //     animator = GetComponentInChildren<Animator>();
    }

    public void PlaceOnTile(BoardPath board, int index)
    {
        TileIndex = Mathf.Clamp(index, 0, board.LastIndex);
        transform.position = board.tiles[TileIndex].position + Vector3.up * heightOffset;
    }

    public IEnumerator MoveSteps(BoardPath board, int steps)
    {
        if (IsMoving) yield break;
        IsMoving = true;

        // SetRun(true); // 🔓 enable later

        for (int i = 0; i < steps; i++)
        {
            if (TileIndex >= board.LastIndex)
                break;

            TileIndex++;
            Vector3 target =
                board.tiles[TileIndex].position +
                Vector3.up * heightOffset;

            yield return MoveTo(target, stepTime);
        }

        // SetRun(false); // 🔓 enable later
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

    /*
    void SetRun(bool running)
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(runParam))
            animator.SetBool(runParam, running);

        if (!string.IsNullOrEmpty(idleParam))
            animator.SetBool(idleParam, !running);
    }
    */
}
