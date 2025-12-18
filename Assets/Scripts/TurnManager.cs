using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameObject[] players;   // visi player objekti uz board
    public DiceRollScript dice;    // tavs esošais dice
    public int currentPlayerIndex = 0;

    public bool isTurnActive = true;

    void Start()
    {
        SetActivePlayer(0);
    }

    public void DiceRolled()
    {
        if (!isTurnActive) return;

        isTurnActive = false;

        int steps = int.Parse(dice.diceFaceNum);

        // Te vēlāk pieslēgsim kustību
        Debug.Log("Player " + currentPlayerIndex + " rolled " + steps);

        Invoke(nameof(NextTurn), 1.0f); // simulē gājiena beigas
    }

    void NextTurn()
    {
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Length)
            currentPlayerIndex = 0;

        SetActivePlayer(currentPlayerIndex);
        isTurnActive = true;
    }

    void SetActivePlayer(int index)
    {
        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetActive(i == index);
        }

        Debug.Log("Now playing: Player " + index);
    }
}
