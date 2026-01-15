using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession I { get; private set; }

    [System.Serializable]
    public class PlayerInfo
    {
        public string name;
        public int characterIndex;
    }

    [Header("Runtime players for the current match")]
    public List<PlayerInfo> players = new List<PlayerInfo>();

    public int PlayerCount => players.Count;

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call this from your start menu Play() (or before loading the game scene).
    /// Builds the player list: Player 0 is the human, others are random.
    /// </summary>
    public void BuildPlayersFromPrefs()
    {
        players.Clear();

        int selectedCharacter = PlayerPrefs.GetInt("SelectedCharacter", 0);
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        int playerCount = Mathf.Clamp(PlayerPrefs.GetInt("PlayerCount", 2), 2, 7);

        // Make a deterministic shuffle so each new match feels “fresh”
        int seed = System.Environment.TickCount;
        var shuffled = NamePool.GetShuffled(seed);

        // Ensure no duplicate of human name
        shuffled.RemoveAll(n => n.Equals(playerName, System.StringComparison.OrdinalIgnoreCase));

        // Human player always index 0
        players.Add(new PlayerInfo { name = playerName, characterIndex = selectedCharacter });

        // Bots
        for (int i = 1; i < playerCount; i++)
        {
            string botName = (i - 1 < shuffled.Count) ? shuffled[i - 1] : $"Bot{i}";
            int botChar = Random.Range(0, 999999) % 999; // just to vary; you can clamp to prefab count later
            players.Add(new PlayerInfo { name = botName, characterIndex = botChar });
        }
    }
}
