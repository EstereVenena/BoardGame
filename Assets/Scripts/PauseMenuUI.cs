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

    [Header("Scene Rules")]
    [Tooltip("Build Index of Main Menu scene (usually 0). In this scene cursor will always be visible/unlocked.")]
    public int mainMenuSceneIndex = 0;

    private bool paused;

    bool IsMainMenuScene()
    {
        return SceneManager.GetActiveScene().buildIndex == mainMenuSceneIndex;
    }

    void Awake()
    {
        // Never start stuck paused
        Time.timeScale = 1f;

        // Main menu should never lock cursor
        if (IsMainMenuScene())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            ApplyCursor(false);
        }
    }

    void Start()
    {
        if (pausePanelRoot) pausePanelRoot.SetActive(false);
        if (settingsPanelRoot) settingsPanelRoot.SetActive(false);
        if (leaderboardPanelRoot) leaderboardPanelRoot.SetActive(false);

        paused = false;
        Time.timeScale = 1f;

        if (IsMainMenuScene())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            ApplyCursor(false);
        }
    }

    void Update()
    {
        // In main menu we don't want ESC to pause-game logic
        if (IsMainMenuScene()) return;

        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (leaderboardPanelRoot != null && leaderboardPanelRoot.activeSelf)
        {
            leaderboardPanelRoot.SetActive(false);
            ShowPausePanelOnly();
            return;
        }

        if (settingsPanelRoot != null && settingsPanelRoot.activeSelf)
        {
            settingsPanelRoot.SetActive(false);
            ShowPausePanelOnly();
            return;
        }

        SetPaused(!paused);
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetPaused(bool on)
    {
        paused = on;

        if (pausePanelRoot != null)
            pausePanelRoot.SetActive(on);

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
        // Main menu: always visible/unlocked
        if (IsMainMenuScene())
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (isPaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = !lockCursorWhenUnpaused;
            Cursor.lockState = lockCursorWhenUnpaused ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }

    private void ShowPausePanelOnly()
    {
        paused = true;
        Time.timeScale = 0f;

        if (pausePanelRoot) pausePanelRoot.SetActive(true);
        ApplyCursor(true);
    }

    public void Continue() => SetPaused(false);

    public void OpenLeaderboard()
    {
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
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Application.Quit();
    }
}
