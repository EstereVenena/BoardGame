using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnGameManager : MonoBehaviour
{
    [Header("References")]
    public DiceRollScript dice;
    public BoardPath board;

    [Header("Spawning")]
    public Transform spawnPoint;
    public GameObject[] pawnPrefabs;   // character prefabs (must have Pawn + NameScript, or they'll be added)

    [Header("UI")]
    public WinScreenUI winUI;

    [Header("Rules")]
    public bool extraTurnOnSix = true;

    // runtime
    readonly List<Pawn> pawns = new List<Pawn>();
    int currentPlayer = 0;
    bool isMoving = false;
    bool gameEnded = false;

    void Start()
    {
        if (dice == null) dice = FindFirstObjectByType<DiceRollScript>();
        if (board == null) board = FindFirstObjectByType<BoardPath>();

        if (GameSession.I == null)
            Debug.LogError("GameSession not found! Add GameSession object in Start Menu scene (DontDestroyOnLoad).");

        // Build players if not built (failsafe)
        if (GameSession.I != null && GameSession.I.players.Count == 0)
            GameSession.I.BuildPlayersFromPrefs();

        SpawnAllPlayers();
        PlaceAllAtStart();

        dice.OnDiceLanded += OnDiceLanded;
        BeginTurn();
    }

    void OnDestroy()
    {
        if (dice != null) dice.OnDiceLanded -= OnDiceLanded;
    }
void SpawnPawns(int count)
{
    pawns.Clear();

    for (int i = 0; i < count; i++)
    {
        var prefab = pawnPrefabs[i % pawnPrefabs.Length];
        var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        var pawn = go.GetComponent<Pawn>();
        if (pawn == null) pawn = go.AddComponent<Pawn>();

        // ✅ lane offset: centered around 0, no drift
        Vector3 laneOffset = new Vector3((i - (count - 1) / 2f) * 0.22f, 0f, 0f);
        pawn.SetTileOffset(laneOffset);

        pawns.Add(pawn);
    }
}

    void SpawnAllPlayers()
    {
        pawns.Clear();

        int count = (GameSession.I != null) ? GameSession.I.PlayerCount : Mathf.Clamp(PlayerPrefs.GetInt("PlayerCount", 2), 2, 7);
        Vector3 basePos = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            // Pick prefab based on player character index
            int prefabIndex = 0;
            string pname = $"Player{i + 1}";

            if (GameSession.I != null)
            {
                prefabIndex = Mathf.Abs(GameSession.I.players[i].characterIndex) % pawnPrefabs.Length;
                pname = GameSession.I.players[i].name;
            }

            var prefab = pawnPrefabs[prefabIndex];
            var pos = basePos + new Vector3(i * 0.25f, 0f, i * 0.12f);

            GameObject go = Instantiate(prefab, pos, Quaternion.identity);

            var pawn = go.GetComponent<Pawn>();
            if (pawn == null) pawn = go.AddComponent<Pawn>();

            var ns = go.GetComponent<NameScript>();
            if (ns == null) ns = go.AddComponent<NameScript>();
            ns.SetName(pname);

            pawns.Add(pawn);
        }
    }

    void PlaceAllAtStart()
    {
        foreach (var p in pawns)
            p.PlaceOnTile(board, 0);
    }

    void BeginTurn()
    {
        if (gameEnded) return;

        dice.SetLocked(false);
        Debug.Log($"TURN: {GetPlayerName(currentPlayer)} roll!");
    }

    void OnDiceLanded(int value)
    {
        if (gameEnded) return;
        if (isMoving) return;

        StartCoroutine(DoMove(value));
    }

    IEnumerator DoMove(int steps)
    {
        isMoving = true;
        dice.SetLocked(true);

        Pawn pawn = pawns[currentPlayer];
        yield return pawn.MoveSteps(board, steps);

        // Win condition
        if (pawn.TileIndex >= board.LastIndex)
        {
            gameEnded = true;

            string winnerName = GetPlayerName(currentPlayer);

            // Save win
            LeaderboardStore.AddWin(winnerName);

            Debug.Log($"🏆 WINNER: {winnerName}");

            if (winUI != null)
                winUI.ShowWinner(winnerName);

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

    string GetPlayerName(int index)
    {
        if (GameSession.I != null && index >= 0 && index < GameSession.I.players.Count)
            return GameSession.I.players[index].name;

        return $"Player {index + 1}";
    }
}
