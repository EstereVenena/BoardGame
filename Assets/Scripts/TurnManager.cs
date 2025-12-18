using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Players")]
    public GameObject[] players;

    [Header("Board")]
    public Transform boardTilesRoot;

    [Header("Dice")]
    public DiceRollScript dice;

    public int currentPlayerIndex = 0;
    bool isInitialized = false;

    void Start()
    {
        Invoke(nameof(InitPlayers), 0.2f);
    }

    void InitPlayers()
    {
        if (players == null || players.Length == 0)
            players = GameObject.FindGameObjectsWithTag("Player");

        if (players == null || players.Length == 0)
        {
            Debug.LogError("TurnManager: No players found.");
            return;
        }

        if (boardTilesRoot == null)
        {
            var obj = GameObject.Find("BoardTiles");
            if (obj != null)
                boardTilesRoot = obj.transform;
            else
            {
                Debug.LogError("TurnManager: BoardTiles not found in scene!");
                return;
            }
        }

        // ✅ PIEŠĶIRAM TILE ROOT + INIT
        foreach (var p in players)
        {
            if (p == null) continue;

            var mover = p.GetComponent<PlayerMoverBoard>();
            if (mover != null)
            {
                mover.tilesRoot = boardTilesRoot;
                mover.InitTiles();
            }
        }

        isInitialized = true;
        currentPlayerIndex = 0;

        Debug.Log("TurnManager initialized. Players found: " + players.Length);
    }

    public void DiceRolled()
    {
        Debug.Log("DiceRolled CALLED");
        


        if (!isInitialized) return;

        if (!int.TryParse(dice.diceFaceNum, out int steps))
            return;

        var anim = players[currentPlayerIndex].GetComponent<Animator>();
        if (anim) anim.SetTrigger("special");

        var mover = players[currentPlayerIndex].GetComponent<PlayerMoverBoard>();
        if (mover == null) return;

        mover.DoTurn(steps, OnPlayerFinishedMove);
        Debug.Log("STEPS = " + steps);
    }

    void OnPlayerFinishedMove()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Length)
            currentPlayerIndex = 0;
    }
}
