using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Players")]
    [Range(2, 7)]
    public int playersCount = 2;

    private enum State
    {
        DecidingFirst,
        NormalTurn
    }

    private State state;

    private int currentPlayer = 0;

    private List<int> rollOffPlayers = new List<int>();
    private List<int> bestPlayers = new List<int>();

    private int rollOffIndex = 0;
    private int bestRoll = -1;

    void Start()
    {
        StartFirstPlayerRollOff();
    }

    // ============================
    // FIRST PLAYER ROLL-OFF
    // ============================
    void StartFirstPlayerRollOff()
    {
        state = State.DecidingFirst;

        rollOffPlayers.Clear();
        for (int i = 0; i < playersCount; i++)
            rollOffPlayers.Add(i);

        ResetRollOffRound();

        Debug.Log("Roll-off started.");
        Debug.Log($"Player {rollOffPlayers[0] + 1}, roll the dice!");
    }

    void ResetRollOffRound()
    {
        rollOffIndex = 0;
        bestRoll = -1;
        bestPlayers.Clear();
    }

    // ============================
    // UI BUTTON CALL
    // ============================
    public void OnRollPressed()
    {
        if (state == State.DecidingFirst)
        {
            HandleRollOff();
            return;
        }

        Debug.Log("Normal gameplay roll (to be added later).");
    }

    // ============================
    // ROLL-OFF LOGIC
    // ============================
    void HandleRollOff()
    {
        int playerId = rollOffPlayers[rollOffIndex];

        // TEMP dice logic (safe)
        int roll = Random.Range(1, 7);

        Debug.Log($"Player {playerId + 1} rolled {roll}");

        if (roll > bestRoll)
        {
            bestRoll = roll;
            bestPlayers.Clear();
            bestPlayers.Add(playerId);
        }
        else if (roll == bestRoll)
        {
            bestPlayers.Add(playerId);
        }

        rollOffIndex++;

        if (rollOffIndex < rollOffPlayers.Count)
        {
            Debug.Log($"Next: Player {rollOffPlayers[rollOffIndex] + 1}, roll!");
            return;
        }

        // Resolve result
        if (bestPlayers.Count == 1)
        {
            currentPlayer = bestPlayers[0];
            Debug.Log($"Player {currentPlayer + 1} starts the game!");

            state = State.NormalTurn;
            BeginTurn();
        }
        else
        {
            Debug.Log("Tie detected! Rerolling only tied players.");

            rollOffPlayers = new List<int>(bestPlayers);
            ResetRollOffRound();

            Debug.Log($"Reroll: Player {rollOffPlayers[0] + 1}, roll!");
        }
    }

    // ============================
    // GAME START
    // ============================
    void BeginTurn()
    {
        Debug.Log($"Game started. Player {currentPlayer + 1}'s turn.");
    }
}
