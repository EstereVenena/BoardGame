using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SimpleSettingsScript with file logging hooks.
/// REQUIREMENT: Add GameFileLogger.cs to project and place a GameObject with GameFileLogger in the first scene.
/// </summary>
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
    public GameObject applyRoot;
    public Button applyButton;

    [Header("Resolution Filtering")]
    public bool useCommonResolutionList = true;

    [Header("Behavior")]
    [Tooltip("If ON, fullscreen will use the desktop resolution (recommended). Dropdown becomes non-interactable while fullscreen is ON.")]
    public bool fullscreenUsesDesktopResolution = true;

    [Header("Debug")]
    public bool debugLogs = true;

    // Common resolutions
    private readonly Vector2Int[] commonResolutions =
    {
        new Vector2Int(640, 480),
        new Vector2Int(800, 600),
        new Vector2Int(1024, 768),
        new Vector2Int(1152, 864),
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160),
    };

    private readonly List<Resolution> supportedResolutions = new List<Resolution>();

    private bool isInitializing;
    private bool suppressUIEvents;

    // Applied (currently active)
    private int appliedW, appliedH;
    private bool appliedFullscreen;

    // Pending (selected but not applied yet)
    private int pendingW, pendingH;
    private bool pendingFullscreen;

    // ---------- Build-safe "unstick" hotkey ----------
    // Hold CTRL+SHIFT and press F8 to wipe display prefs (works in builds too)
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F8))
        {
            ClearDisplayPrefs();
            StartCoroutine(ReInitNextFrame());
        }
    }

    private IEnumerator ReInitNextFrame()
    {
        yield return null;
        Init();
    }

    private void ClearDisplayPrefs()
    {
        PlayerPrefs.DeleteKey("BootW");
        PlayerPrefs.DeleteKey("BootH");
        PlayerPrefs.DeleteKey("Fullscreen");
        PlayerPrefs.Save();

        LogEvent("PREFS_CLEARED", ("keys", "BootW/BootH/Fullscreen"));
        if (debugLogs) Debug.Log("[Settings] CTRL+SHIFT+F8 => Cleared BootW/BootH/Fullscreen PlayerPrefs.");
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        isInitializing = true;
        suppressUIEvents = true;

        LogEvent("INIT_BEGIN",
            ("screenSize", $"{Screen.width}x{Screen.height}"),
            ("screenMode", Screen.fullScreenMode),
            ("screenFS", Screen.fullScreen),
            ("persistentPath", Application.persistentDataPath)
        );

        // Build dropdown options first
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
        appliedFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 1;

        // Pending starts = applied
        pendingW = appliedW;
        pendingH = appliedH;
        pendingFullscreen = appliedFullscreen;

        // Sync UI without firing events
        if (fullscreenToggle)
            fullscreenToggle.SetIsOnWithoutNotify(appliedFullscreen);

        if (resolutionDropdown)
        {
            int idx = FindIndexByWH(appliedW, appliedH);
            if (idx < 0) idx = FindClosestResolutionIndex(appliedW, appliedH);

            resolutionDropdown.SetValueWithoutNotify(idx);
            resolutionDropdown.RefreshShownValue();
        }

        // Hook apply button
        if (applyButton)
        {
            applyButton.onClick.RemoveListener(ApplyPendingDisplay);
            applyButton.onClick.AddListener(ApplyPendingDisplay);
        }

        suppressUIEvents = false;
        isInitializing = false;

        // Apply once at startup
        ApplyDisplayNow(appliedW, appliedH, appliedFullscreen, source: "STARTUP_APPLY");

        // Lock dropdown if fullscreen uses desktop res
        UpdateResolutionDropdownLock();

        UpdateApplyUI();

        DumpState("INIT_DONE");
        LogEvent("INIT_DONE",
            ("applied", $"{appliedW}x{appliedH}"),
            ("appliedFS", appliedFullscreen),
            ("pending", $"{pendingW}x{pendingH}"),
            ("pendingFS", pendingFullscreen),
            ("dropdownCount", supportedResolutions.Count)
        );
    }

    // ---------------- UI Events ----------------

    public void OnMusicSlider(float v)
    {
        if (isInitializing || suppressUIEvents) return;
        LogEvent("UI_MUSIC_SLIDER", ("value", v));
        ApplyMusic(v);
    }

    public void OnSfxSlider(float v)
    {
        if (isInitializing || suppressUIEvents) return;
        LogEvent("UI_SFX_SLIDER", ("value", v));
        ApplySfx(v);
    }

    public void OnResolutionChanged(int dropdownIndex)
    {
        if (isInitializing || suppressUIEvents) return;

        LogEvent("UI_RESOLUTION_CHANGED", ("index", dropdownIndex));

        // If fullscreen uses desktop res, dropdown is meaningless
        if (pendingFullscreen && fullscreenUsesDesktopResolution)
        {
            LogEvent("UI_RESOLUTION_IGNORED", ("reason", "fullscreenUsesDesktopResolution"));
            if (debugLogs) Debug.Log("[Settings] Resolution change ignored (Fullscreen uses desktop resolution).");
            return;
        }

        if (supportedResolutions.Count == 0) return;

        dropdownIndex = Mathf.Clamp(dropdownIndex, 0, supportedResolutions.Count - 1);
        var r = supportedResolutions[dropdownIndex];

        pendingW = r.width;
        pendingH = r.height;

        LogEvent("PENDING_RES_SET",
            ("pending", $"{pendingW}x{pendingH}"),
            ("applied", $"{appliedW}x{appliedH}")
        );

        if (debugLogs)
            Debug.Log($"[Settings] Dropdown changed index={dropdownIndex} => pending={pendingW}x{pendingH} (applied={appliedW}x{appliedH})");

        UpdateApplyUI();
    }

    public void OnFullscreenChanged(bool on)
    {
        if (isInitializing || suppressUIEvents) return;

        LogEvent("UI_FULLSCREEN_CHANGED", ("on", on));

        pendingFullscreen = on;

        // Rebuild dropdown safely
        suppressUIEvents = true;

        RebuildResolutionList();
        SetupResolutionDropdownOptions();

        // Keep dropdown aligned to pending res
        int idx = FindIndexByWH(pendingW, pendingH);
        if (idx < 0) idx = FindClosestResolutionIndex(pendingW, pendingH);

        if (resolutionDropdown)
        {
            resolutionDropdown.SetValueWithoutNotify(idx);
            resolutionDropdown.RefreshShownValue();
        }

        suppressUIEvents = false;

        UpdateResolutionDropdownLock();
        UpdateApplyUI();

        DumpState("AFTER_FULLSCREEN_TOGGLE");

        LogEvent("PENDING_FULLSCREEN_SET",
            ("pendingFS", pendingFullscreen),
            ("dropdownCount", supportedResolutions.Count)
        );
    }

    public void ApplyPendingDisplay()
    {
        LogEvent("UI_APPLY_CLICKED", ("hasPending", HasPendingChanges()));

        if (!HasPendingChanges())
        {
            if (debugLogs) Debug.Log("[Settings] Apply clicked but no pending changes.");
            LogEvent("APPLY_ABORTED", ("reason", "no_pending_changes"));
            return;
        }

        int targetW = pendingW;
        int targetH = pendingH;

        // If fullscreen uses desktop resolution, override
        if (pendingFullscreen && fullscreenUsesDesktopResolution)
        {
            var cr = Screen.currentResolution;
            targetW = cr.width;
            targetH = cr.height;

            LogEvent("FULLSCREEN_DESKTOP_OVERRIDE", ("desktop", $"{targetW}x{targetH}"));
        }

        appliedW = targetW;
        appliedH = targetH;
        appliedFullscreen = pendingFullscreen;

        LogEvent("APPLY_WILL_APPLY",
            ("appliedW", appliedW),
            ("appliedH", appliedH),
            ("appliedFS", appliedFullscreen)
        );

        if (debugLogs)
            Debug.Log($"[Settings] APPLY clicked. Applying {appliedW}x{appliedH} fullscreen={appliedFullscreen}");

        ApplyDisplayNow(appliedW, appliedH, appliedFullscreen, source: "APPLY_BUTTON");

        // Save applied values
        PlayerPrefs.SetInt("BootW", appliedW);
        PlayerPrefs.SetInt("BootH", appliedH);
        PlayerPrefs.SetInt("Fullscreen", appliedFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        LogEvent("PREFS_SAVED",
            ("BootW", appliedW),
            ("BootH", appliedH),
            ("Fullscreen", appliedFullscreen ? 1 : 0)
        );

        // Sync pending to applied after apply
        pendingW = appliedW;
        pendingH = appliedH;
        pendingFullscreen = appliedFullscreen;

        UpdateResolutionDropdownLock();
        UpdateApplyUI();

        DumpState("AFTER_APPLY");
        LogEvent("APPLY_DONE",
            ("screenNow", $"{Screen.width}x{Screen.height}"),
            ("modeNow", Screen.fullScreenMode),
            ("fsNow", Screen.fullScreen)
        );
    }

    // ---------------- Internals ----------------

    private bool HasPendingChanges()
    {
        return pendingW != appliedW || pendingH != appliedH || pendingFullscreen != appliedFullscreen;
    }

    private void UpdateApplyUI()
    {
        bool pending = HasPendingChanges();

        // Keep visible
        if (applyRoot) applyRoot.SetActive(true);

        // Only clickable if changes exist
        if (applyButton) applyButton.interactable = pending;

        if (debugLogs)
            Debug.Log($"[Settings] Apply UI -> pending={pending}, applyRootActive={(applyRoot ? applyRoot.activeSelf : false)}, applyInteractable={(applyButton ? applyButton.interactable : false)}");

        LogEvent("APPLY_UI",
            ("pending", pending),
            ("applyInteractable", applyButton ? applyButton.interactable : false)
        );
    }

    private void UpdateResolutionDropdownLock()
    {
        if (!resolutionDropdown) return;

        bool lockDropdown = pendingFullscreen && fullscreenUsesDesktopResolution;
        resolutionDropdown.interactable = !lockDropdown;

        LogEvent("RES_DROPDOWN_LOCK",
            ("locked", lockDropdown),
            ("reason", lockDropdown ? "fullscreenUsesDesktopResolution" : "windowed_or_custom_fullscreen")
        );

        // If locked, make dropdown show desktop res (honest UI)
        if (lockDropdown)
        {
            var cr = Screen.currentResolution;
            int idx = FindIndexByWH(cr.width, cr.height);
            if (idx < 0) idx = FindClosestResolutionIndex(cr.width, cr.height);

            suppressUIEvents = true;
            resolutionDropdown.SetValueWithoutNotify(idx);
            resolutionDropdown.RefreshShownValue();
            suppressUIEvents = false;

            LogEvent("RES_DROPDOWN_FORCED_DESKTOP",
                ("desktop", $"{cr.width}x{cr.height}"),
                ("index", idx)
            );
        }
    }

    private void ApplyDisplayNow(int w, int h, bool fullscreen, string source)
    {
        // If fullscreen uses desktop res, apply desktop res here too (startup-safe)
        if (fullscreen && fullscreenUsesDesktopResolution)
        {
            var cr = Screen.currentResolution;
            w = cr.width;
            h = cr.height;

            appliedW = w;
            appliedH = h;

            LogEvent("DISPLAY_DESKTOP_OVERRIDE",
                ("source", source),
                ("desktop", $"{w}x{h}")
            );
        }

        var mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        LogEvent("DISPLAY_TRY",
            ("source", source),
            ("want", $"{w}x{h}"),
            ("wantFS", fullscreen),
            ("wantMode", mode),
            ("beforeSize", $"{Screen.width}x{Screen.height}"),
            ("beforeFS", Screen.fullScreen),
            ("beforeMode", Screen.fullScreenMode)
        );

        Screen.fullScreenMode = mode;
        Screen.SetResolution(w, h, mode);

        if (debugLogs)
            Debug.Log($"[Settings] Applied {w}x{h} mode={mode}");

        StartCoroutine(LogDisplayAfterApply(source));
    }

    private IEnumerator LogDisplayAfterApply(string source)
    {
        yield return new WaitForEndOfFrame();

        LogEvent("DISPLAY_AFTER_FRAME",
            ("source", source),
            ("afterSize", $"{Screen.width}x{Screen.height}"),
            ("afterFS", Screen.fullScreen),
            ("afterMode", Screen.fullScreenMode)
        );
    }

    private void RebuildResolutionList()
    {
        supportedResolutions.Clear();
        var seen = new HashSet<string>();

        // Add common list first (Editor-safe)
        if (useCommonResolutionList)
        {
            foreach (var wanted in commonResolutions)
            {
                string key = $"{wanted.x}x{wanted.y}";
                if (seen.Add(key))
                    supportedResolutions.Add(new Resolution { width = wanted.x, height = wanted.y });
            }
        }

        // Add system reported resolutions too
        foreach (var r in Screen.resolutions)
        {
            string key = $"{r.width}x{r.height}";
            if (seen.Add(key))
                supportedResolutions.Add(r);
        }

        if (supportedResolutions.Count == 0)
            supportedResolutions.Add(new Resolution { width = Screen.width, height = Screen.height });

        if (debugLogs)
        {
            Debug.Log($"[Settings] supportedResolutions.Count = {supportedResolutions.Count}");
            for (int i = 0; i < supportedResolutions.Count; i++)
                Debug.Log($"[Settings] {i}: {supportedResolutions[i].width}x{supportedResolutions[i].height}");
        }

        // Log the list (truncate if huge)
        LogEvent("RES_LIST_REBUILT", ("count", supportedResolutions.Count));
        int max = Mathf.Min(40, supportedResolutions.Count);
        for (int i = 0; i < max; i++)
            LogEvent("RES_ITEM", ("i", i), ("wh", $"{supportedResolutions[i].width}x{supportedResolutions[i].height}"));
        if (supportedResolutions.Count > max)
            LogEvent("RES_ITEM_TRUNCATED", ("shown", max), ("total", supportedResolutions.Count));
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

        LogEvent("DROPDOWN_OPTIONS_REBUILT", ("count", options.Count));
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
        bool has = musicSource != null;
        if (musicSource) musicSource.volume = v;

        PlayerPrefs.SetFloat("MusicVol", v);
        PlayerPrefs.Save();

        LogEvent("AUDIO_MUSIC_APPLY", ("v", v), ("sourceAssigned", has), ("actualVol", has ? musicSource.volume : -1f));
    }

    private void ApplySfx(float v)
    {
        v = Mathf.Clamp01(v);
        bool has = sfxSource != null;
        if (sfxSource) sfxSource.volume = v;

        PlayerPrefs.SetFloat("SFXVol", v);
        PlayerPrefs.Save();

        LogEvent("AUDIO_SFX_APPLY", ("v", v), ("sourceAssigned", has), ("actualVol", has ? sfxSource.volume : -1f));
    }

    private void DumpState(string tag)
    {
        if (!debugLogs) return;
        Debug.Log($"[Settings] STATE ({tag}) applied={appliedW}x{appliedH} fs={appliedFullscreen} | pending={pendingW}x{pendingH} fs={pendingFullscreen}");
    }

    // ---------------- Logging helpers ----------------

    private void LogEvent(string evt, params (string k, object v)[] fields)
    {
        // Works even if logger not present (safe no-op)
        if (GameFileLogger.I == null) return;
        GameFileLogger.LogEvent(evt, fields);
    }
}
