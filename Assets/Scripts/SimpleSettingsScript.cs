using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleSettingsScript : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("UI")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Apply UI (Button)")]
    [Tooltip("Root object for the checkmark/apply visuals. Shown only when changes are pending.")]
    public GameObject applyRoot;

    [Tooltip("Button that applies pending display settings.")]
    public Button applyButton;

    [Header("Resolution Filtering")]
    [Tooltip("If ON, dropdown will TRY to show common resolutions. If too few are found, it falls back to all unique resolutions.")]
    public bool useCommonResolutionList = true;

    [Header("Debug")]
    public bool debugLogs = true;

    // Common, sane resolutions (edit as you like)
    private readonly Vector2Int[] commonResolutions =
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };

    private readonly List<Resolution> supportedResolutions = new List<Resolution>();
    private bool isInitializing;

    // Applied (currently active)
    private int appliedW, appliedH;
    private bool appliedFullscreen;

    // Pending (selected but not applied yet)
    private int pendingW, pendingH;
    private bool pendingFullscreen;

    void Start()
    {
        isInitializing = true;

        RebuildResolutionList();
        SetupResolutionDropdownOptions();

        // Load audio
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.7f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.7f);

        if (musicSlider) musicSlider.SetValueWithoutNotify(musicVol);
        if (sfxSlider) sfxSlider.SetValueWithoutNotify(sfxVol);

        ApplyMusic(musicVol);
        ApplySfx(sfxVol);

        // Load applied display settings (fallback to current)
        appliedW = PlayerPrefs.GetInt("BootW", Screen.width);
        appliedH = PlayerPrefs.GetInt("BootH", Screen.height);
        appliedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        // Pending starts = applied
        pendingW = appliedW;
        pendingH = appliedH;
        pendingFullscreen = appliedFullscreen;

        // Sync UI to applied values
        if (fullscreenToggle)
            fullscreenToggle.SetIsOnWithoutNotify(appliedFullscreen);

        if (resolutionDropdown)
        {
            int idx = FindIndexByWH(appliedW, appliedH);
            if (idx < 0) idx = FindClosestResolutionIndex(appliedW, appliedH);

            resolutionDropdown.SetValueWithoutNotify(idx);
            resolutionDropdown.RefreshShownValue();
        }

        // Apply once at startup (enforce saved settings in build)
        ApplyDisplayNow(appliedW, appliedH, appliedFullscreen);

        // Hook apply button
        if (applyButton != null)
        {
            applyButton.onClick.RemoveListener(ApplyPendingDisplay);
            applyButton.onClick.AddListener(ApplyPendingDisplay);
        }

        UpdateApplyUI();

        isInitializing = false;

        if (debugLogs) DumpState("START done");
    }

    // ---------------- UI Events ----------------

    public void OnMusicSlider(float v)
    {
        if (isInitializing) return;
        ApplyMusic(v);
    }

    public void OnSfxSlider(float v)
    {
        if (isInitializing) return;
        ApplySfx(v);
    }

    public void OnResolutionChanged(int dropdownIndex)
    {
        if (isInitializing) return;

        dropdownIndex = Mathf.Clamp(dropdownIndex, 0, supportedResolutions.Count - 1);
        var r = supportedResolutions[dropdownIndex];

        pendingW = r.width;
        pendingH = r.height;

        if (debugLogs)
        {
            Debug.Log($"[Settings] Dropdown changed index={dropdownIndex} => pending={pendingW}x{pendingH} (applied={appliedW}x{appliedH})");
        }

        UpdateApplyUI();
    }

    public void OnFullscreenChanged(bool on)
    {
        if (isInitializing) return;

        pendingFullscreen = on;

        // Fullscreen changes can affect available resolutions on some systems.
        // Rebuild to keep the dropdown valid.
        RebuildResolutionList();
        SetupResolutionDropdownOptions();

        // Keep dropdown aligned to pending W/H after rebuild
        int idx = FindIndexByWH(pendingW, pendingH);
        if (idx < 0) idx = FindClosestResolutionIndex(pendingW, pendingH);

        if (resolutionDropdown)
        {
            resolutionDropdown.SetValueWithoutNotify(idx);
            resolutionDropdown.RefreshShownValue();
        }

        if (debugLogs)
        {
            Debug.Log($"[Settings] Pending fullscreen = {pendingFullscreen}");
            DumpState("After fullscreen toggle");
        }

        UpdateApplyUI();
    }

    public void ApplyPendingDisplay()
    {
        if (!HasPendingChanges())
        {
            if (debugLogs) Debug.Log("[Settings] Apply clicked but no pending changes.");
            return;
        }

        appliedW = pendingW;
        appliedH = pendingH;
        appliedFullscreen = pendingFullscreen;

        if (debugLogs)
            Debug.Log($"[Settings] APPLY clicked. Applying {appliedW}x{appliedH} fullscreen={appliedFullscreen}");

        ApplyDisplayNow(appliedW, appliedH, appliedFullscreen);

        // Save
        PlayerPrefs.SetInt("BootW", appliedW);
        PlayerPrefs.SetInt("BootH", appliedH);
        PlayerPrefs.SetInt("Fullscreen", appliedFullscreen ? 1 : 0);

        // Also store index for convenience (not relied on)
        int appliedIndex = FindIndexByWH(appliedW, appliedH);
        if (appliedIndex >= 0) PlayerPrefs.SetInt("ResIndex", appliedIndex);

        PlayerPrefs.Save();

        UpdateApplyUI();

        if (debugLogs) DumpState("After APPLY");
    }

    // ---------------- Internals ----------------

    private bool HasPendingChanges()
    {
        return pendingW != appliedW || pendingH != appliedH || pendingFullscreen != appliedFullscreen;
    }

    private void UpdateApplyUI()
    {
        bool pending = HasPendingChanges();

        // Your choice:
        // A) Show/hide apply when pending:
        if (applyRoot) applyRoot.SetActive(pending);

        // B) If you want it ALWAYS visible (recommended UI), comment out the line above and uncomment below:
        // if (applyRoot) applyRoot.SetActive(true);

        if (applyButton) applyButton.interactable = pending;

        if (debugLogs)
            Debug.Log($"[Settings] Apply UI -> pending={pending}, applyRootActive={(applyRoot ? applyRoot.activeSelf : false)}, applyInteractable={(applyButton ? applyButton.interactable : false)}");
    }

    private void ApplyDisplayNow(int w, int h, bool fullscreen)
    {
        // Most reliable in builds:
        var mode = fullscreen ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;

        Screen.fullScreenMode = mode;
        Screen.SetResolution(w, h, mode);

        if (debugLogs)
            Debug.Log($"[Settings] Applied {w}x{h} mode={mode}");
    }

    private void RebuildResolutionList()
    {
        supportedResolutions.Clear();
        var supported = Screen.resolutions;

        // 1) Try common list first
        if (useCommonResolutionList)
        {
            foreach (var wanted in commonResolutions)
            {
                for (int i = 0; i < supported.Length; i++)
                {
                    if (supported[i].width == wanted.x && supported[i].height == wanted.y)
                    {
                        supportedResolutions.Add(supported[i]);
                        break;
                    }
                }
            }
        }

        // 2) If too few, fall back to unique list (prevents “everything becomes 1280x720”)
        if (supportedResolutions.Count < 2)
        {
            supportedResolutions.Clear();

            var seen = new HashSet<string>();
            foreach (var r in supported)
            {
                string key = $"{r.width}x{r.height}";
                if (seen.Add(key))
                    supportedResolutions.Add(r);
            }
        }

        // 3) Final fallback
        if (supportedResolutions.Count == 0)
            supportedResolutions.Add(new Resolution { width = Screen.width, height = Screen.height });

        if (debugLogs)
        {
            Debug.Log($"[Settings] supportedResolutions.Count = {supportedResolutions.Count}");
            for (int i = 0; i < supportedResolutions.Count; i++)
                Debug.Log($"[Settings]  {i}: {supportedResolutions[i].width}x{supportedResolutions[i].height}");
        }
    }

    private void SetupResolutionDropdownOptions()
    {
        if (!resolutionDropdown) return;

        resolutionDropdown.ClearOptions();
        var options = new List<string>(supportedResolutions.Count);

        for (int i = 0; i < supportedResolutions.Count; i++)
            options.Add($"{supportedResolutions[i].width} x {supportedResolutions[i].height}");

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();

        if (debugLogs)
            Debug.Log("[Settings] Dropdown options rebuilt.");
    }

    private int FindIndexByWH(int w, int h)
    {
        for (int i = 0; i < supportedResolutions.Count; i++)
        {
            if (supportedResolutions[i].width == w && supportedResolutions[i].height == h)
                return i;
        }
        return -1;
    }

    private int FindClosestResolutionIndex(int w, int h)
    {
        int best = 0;
        int bestDiff = int.MaxValue;

        for (int i = 0; i < supportedResolutions.Count; i++)
        {
            int diff = Mathf.Abs(supportedResolutions[i].width - w) + Mathf.Abs(supportedResolutions[i].height - h);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
            }
        }
        return best;
    }

    private void ApplyMusic(float v)
    {
        v = Mathf.Clamp01(v);
        if (musicSource) musicSource.volume = v;
        PlayerPrefs.SetFloat("MusicVol", v);
        PlayerPrefs.Save();
    }

    private void ApplySfx(float v)
    {
        v = Mathf.Clamp01(v);
        if (sfxSource) sfxSource.volume = v;
        PlayerPrefs.SetFloat("SFXVol", v);
        PlayerPrefs.Save();
    }

    private void DumpState(string tag)
    {
        Debug.Log($"[Settings] STATE ({tag}) applied={appliedW}x{appliedH} fs={appliedFullscreen} | pending={pendingW}x{pendingH} fs={pendingFullscreen}");
    }
}
