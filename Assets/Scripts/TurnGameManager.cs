using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGameManager : MonoBehaviour
{
    [Header("References")]
    public DiceRollScript dice;
    public BoardPath board;

    [Header("Players (assign pawn prefabs OR existing pawns)")]
    public Transform spawnPoint;
    public GameObject[] pawnPrefabs;         // optional: spawn from prefabs
    public List<Pawn> pawns = new List<Pawn>(); // if you already have pawns in scene, drag them here

    [Header("Game Rules")]
    public bool extraTurnOnSix = true;

    int currentPlayer = 0;
    bool isMoving = false;

    void Start()
    {
        if (dice == null) dice = FindFirstObjectByType<DiceRollScript>();
        if (board == null) board = FindFirstObjectByType<BoardPath>();

        // Spawn pawns if none provided
        if (pawns.Count == 0 && pawnPrefabs != null && pawnPrefabs.Length > 0)
            SpawnPawns();

        // Place all pawns at Tile0
        foreach (var p in pawns)
            p.PlaceOnTile(board, 0);

        // Subscribe to dice result
        dice.OnDiceLanded += OnDiceLanded;

        BeginTurn();
    }

    void OnDestroy()
    {
        if (dice != null) dice.OnDiceLanded -= OnDiceLanded;
    }

    void SpawnPawns()
    {
        Vector3 basePos = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        for (int i = 0; i < pawnPrefabs.Length; i++)
        {
            var go = Instantiate(pawnPrefabs[i], basePos + new Vector3(i * 0.25f, 0, i * 0.1f), Quaternion.identity);
            var pawn = go.GetComponent<Pawn>();
            if (pawn == null) pawn = go.AddComponent<Pawn>();
            pawns.Add(pawn);
        }
    }

    void BeginTurn()
    {
        Debug.Log($"TURN: Player {currentPlayer + 1} roll!");
        dice.SetLocked(false); // allow rolling
    }

    void OnDiceLanded(int value)
    {
        if (isMoving) return; // safety
        StartCoroutine(DoMove(value));
    }

    IEnumerator DoMove(int steps)
    {
        isMoving = true;

        // lock dice so player can't spam roll while moving
        dice.SetLocked(true);

        Pawn pawn = pawns[currentPlayer];
        yield return pawn.MoveSteps(board, steps);

        // Win condition: reached last tile
        if (pawn.TileIndex >= board.LastIndex)
        {
            Debug.Log($"🏆 WINNER: Player {currentPlayer + 1}!");
            yield break;
        }

        // Extra turn on 6
        if (extraTurnOnSix && steps == 6)
        {
            isMoving = false;
            BeginTurn();
            yield break;
        }

        // Next player
        currentPlayer = (currentPlayer + 1) % pawns.Count;

        isMoving = false;
        BeginTurn();
    }
}
