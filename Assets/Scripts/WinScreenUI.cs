using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject panelRoot;     // the whole win card object
    public TMP_Text winnerNameText;  // "PLAYER NAME" text
    public TMP_Text subText;         // "YOU WIN" text (optional)

    void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void ShowWinner(string winnerName)
    {
        if (panelRoot != null) panelRoot.SetActive(true);

        if (winnerNameText != null)
            winnerNameText.text = winnerName;

        if (subText != null)
            subText.text = "YOU WIN";

        // freeze gameplay
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    // Button: Home
    public void GoHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // Button: Restart
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
