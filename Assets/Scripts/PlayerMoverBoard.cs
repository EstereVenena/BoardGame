using UnityEngine;
using System;
using System.Collections;

public class PlayerMoverBoard : MonoBehaviour
{
    [Header("Board Tiles")]
    public Transform tilesRoot;     // BoardTiles no scēnes
    private Transform[] tiles;

    [Header("Movement")]
    public float moveSpeed = 5f;

    int tileIndex = 0;
    Animator anim;

    void Awake()
{
    anim = GetComponent<Animator>();

    // 🔑 nodrošina, ka starts ir uz Tile0
    if (tilesRoot != null && tilesRoot.childCount > 0)
        transform.position = tilesRoot.GetChild(0).position;
}
    // ✅ IZSAUC NO TURNMANAGER
    public void InitTiles()
    {
        if (tilesRoot == null)
        {
            Debug.LogError("PlayerMoverBoard: tilesRoot not assigned on " + name);
            return;
        }

        tiles = new Transform[tilesRoot.childCount];
        for (int i = 0; i < tilesRoot.childCount; i++)
            tiles[i] = tilesRoot.GetChild(i);
    }

    public void DoTurn(int steps, Action onFinished)
    {
        StopAllCoroutines();
        StartCoroutine(MoveSteps(steps, onFinished));
    }

    IEnumerator MoveSteps(int steps, Action onFinished)
    {
        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("PlayerMoverBoard: tiles not initialized on " + name);
            onFinished?.Invoke();
            yield break;
        }

        if (anim) anim.SetBool("isWalking", true);

        for (int i = 0; i < steps; i++)
        {
            if (tileIndex >= tiles.Length - 1)
                break;

            tileIndex++;

            Vector3 target = tiles[tileIndex].position;

            while (Vector3.Distance(transform.position, target) > 0.02f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        if (anim) anim.SetBool("isWalking", false);

        onFinished?.Invoke();
    }
}
