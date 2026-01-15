using System.Text;
using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public TMP_Text listText;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (listText == null) return;

        var sorted = LeaderboardStore.GetSorted();
        var sb = new StringBuilder();

        for (int i = 0; i < sorted.Count; i++)
        {
            string medal =
                i == 0 ? "🥇" :
                i == 1 ? "🥈" :
                i == 2 ? "🥉" : "  ";

            sb.AppendLine($"{medal}  {sorted[i].name} — {sorted[i].wins}");
        }

        if (sorted.Count == 0)
            sb.AppendLine("No wins yet. Go bully the bots.");

        listText.text = sb.ToString();
    }
}
