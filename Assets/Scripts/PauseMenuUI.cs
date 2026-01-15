using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject pausePanelRoot;
    public GameObject settingsPanelRoot;
    public GameObject leaderboardPanelRoot;

    [Header("Cursor (recommended for PC)")]
    public bool lockCursorWhenUnpaused = true;

    private bool paused;

    void Awake()
    {
        // Safety: if this object survives scene changes or gets re-enabled,
        // make sure the game isn't stuck paused.
        Time.timeScale = 1f;
        ApplyCursor(false);
    }

    void Start()
    {
        // Close everything on start
        if (pausePanelRoot) pausePanelRoot.SetActive(false);
        if (settingsPanelRoot) settingsPanelRoot.SetActive(false);
        if (leaderboardPanelRoot) leaderboardPanelRoot.SetActive(false);

        paused = false;
        Time.timeScale = 1f;
        ApplyCursor(false);
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // If leaderboard open -> close it first
        if (leaderboardPanelRoot != null && leaderboardPanelRoot.activeSelf)
        {
            leaderboardPanelRoot.SetActive(false);
            ShowPausePanelOnly(); // back to pause menu
            return;
        }

        // If settings open -> close it first
        if (settingsPanelRoot != null && settingsPanelRoot.activeSelf)
        {
            settingsPanelRoot.SetActive(false);
            ShowPausePanelOnly(); // back to pause menu
            return;
        }

        // Otherwise toggle pause
        SetPaused(!paused);
    }

    void OnDisable()
    {
        // Never allow "stuck paused" or "lost cursor" if object gets disabled
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- Core pause control ---
    public void SetPaused(bool on)
    {
        paused = on;

        // Pause panel visible when paused
        if (pausePanelRoot != null)
            pausePanelRoot.SetActive(on);

        // Close sub-panels when unpausing
        if (!on)
        {
            if (settingsPanelRoot) settingsPanelRoot.SetActive(false);
            if (leaderboardPanelRoot) leaderboardPanelRoot.SetActive(false);
        }

        Time.timeScale = on ? 0f : 1f;
        ApplyCursor(on);
    }

    private void ApplyCursor(bool isPaused)
    {
        if (isPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = !lockCursorWhenUnpaused; // if locking, hide; if not, show
            Cursor.lockState = lockCursorWhenUnpaused ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private void ShowPausePanelOnly()
    {
        // If you closed a sub-menu while paused, keep the pause panel visible
        paused = true;
        Time.timeScale = 0f;

        if (pausePanelRoot) pausePanelRoot.SetActive(true);
        ApplyCursor(true);
    }

    // --- UI Buttons ---
    public void Continue()
    {
        SetPaused(false);
    }

    public void OpenLeaderboard()
    {
        // Opening menus should pause
        if (!paused) SetPaused(true);

        if (pausePanelRoot) pausePanelRoot.SetActive(false);
        if (settingsPanelRoot) settingsPanelRoot.SetActive(false);

        if (leaderboardPanelRoot) leaderboardPanelRoot.SetActive(true);

        ApplyCursor(true);
    }

    public void OpenSettings()
    {
        if (!paused) SetPaused(true);

        if (pausePanelRoot) pausePanelRoot.SetActive(false);
        if (leaderboardPanelRoot) leaderboardPanelRoot.SetActive(false);

        if (settingsPanelRoot) settingsPanelRoot.SetActive(true);

        ApplyCursor(true);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(0);
    }

    // Optional: hook this to a UI Quit button
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Application.Quit();
    }
}
