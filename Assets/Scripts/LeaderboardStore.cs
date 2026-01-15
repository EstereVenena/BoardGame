using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class LeaderboardStore
{
    [System.Serializable]
    private class Entry
    {
        public string name;
        public int wins;
    }

    [System.Serializable]
    private class SaveData
    {
        public List<Entry> entries = new List<Entry>();
    }

    private static readonly string FilePath =
        Path.Combine(Application.persistentDataPath, "leaderboard.json");

    private static Dictionary<string, int> _wins;

    private static void EnsureLoaded()
    {
        if (_wins != null) return;

        _wins = new Dictionary<string, int>();

        if (!File.Exists(FilePath))
            return;

        try
        {
            var json = File.ReadAllText(FilePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            if (data?.entries == null) return;

            foreach (var e in data.entries)
            {
                if (string.IsNullOrWhiteSpace(e.name)) continue;
                _wins[e.name] = Mathf.Max(0, e.wins);
            }
        }
        catch
        {
            // If file corrupt, start fresh instead of exploding
            _wins = new Dictionary<string, int>();
        }
    }

    private static void Save()
    {
        var data = new SaveData
        {
            entries = _wins.Select(kv => new Entry { name = kv.Key, wins = kv.Value }).ToList()
        };

        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
    }

    public static void AddWin(string playerName)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Unknown";

        if (!_wins.ContainsKey(playerName))
            _wins[playerName] = 0;

        _wins[playerName]++;
        Save();
    }

    public static List<(string name, int wins)> GetSorted()
    {
        EnsureLoaded();

        return _wins
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }
}
