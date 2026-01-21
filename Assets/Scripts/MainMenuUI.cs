using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("Menu parts")]
    public GameObject panelRoot;          // the big green background panel (keep ON)
    public GameObject buttonsRoot;        // Buttons container (toggle this)

    [Header("Popups")]
    public GameObject settingsRoot;
    public GameObject characterSelectRoot;
    public GameObject leaderboardRoot;

    [Header("Behavior")]
    public bool escGoesBack = true;

    void Awake()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Start()
    {
        ShowMain();
    }

    void Update()
    {
        if (!escGoesBack) return;
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (settingsRoot != null && settingsRoot.activeSelf) { CloseSettings(); return; }
        if (leaderboardRoot != null && leaderboardRoot.activeSelf) { CloseLeaderboard(); return; }
        if (characterSelectRoot != null && characterSelectRoot.activeSelf) { CloseCharacterSelect(); return; }
    }

    public void ShowMain()
    {
        SetActiveSafe(panelRoot, true);      // keep background
        SetActiveSafe(buttonsRoot, true);    // show buttons

        SetActiveSafe(settingsRoot, false);
        SetActiveSafe(characterSelectRoot, false);
        SetActiveSafe(leaderboardRoot, false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        SetActiveSafe(panelRoot, true);
        SetActiveSafe(buttonsRoot, false);   // hide buttons only
        SetActiveSafe(settingsRoot, true);

        SetActiveSafe(characterSelectRoot, false);
        SetActiveSafe(leaderboardRoot, false);
    }

    public void CloseSettings() => ShowMain();

    public void OpenCharacterSelect()
    {
        SetActiveSafe(panelRoot, true);
        SetActiveSafe(buttonsRoot, false);
        SetActiveSafe(characterSelectRoot, true);

        SetActiveSafe(settingsRoot, false);
        SetActiveSafe(leaderboardRoot, false);
    }

    public void CloseCharacterSelect() => ShowMain();

    public void OpenLeaderboard()
    {
        SetActiveSafe(panelRoot, true);
        SetActiveSafe(buttonsRoot, false);
        SetActiveSafe(leaderboardRoot, true);

        SetActiveSafe(settingsRoot, false);
        SetActiveSafe(characterSelectRoot, false);
    }

    public void CloseLeaderboard() => ShowMain();

    public void Quit()
    {
        Application.Quit();
    }

    private void SetActiveSafe(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }
}
