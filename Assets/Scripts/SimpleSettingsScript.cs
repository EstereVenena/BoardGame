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

    private Resolution[] allResolutions;
    private List<Resolution> uniqueResolutions = new List<Resolution>();
    private bool isInitializing;

    void Start()
    {
        isInitializing = true;

        // ---- Resolution options (unique) ----
        allResolutions = Screen.resolutions;
        uniqueResolutions.Clear();

        // Build unique list by width/height only
        HashSet<string> seen = new HashSet<string>();
        foreach (var r in allResolutions)
        {
            string key = $"{r.width}x{r.height}";
            if (seen.Add(key))
                uniqueResolutions.Add(r);
        }

        resolutionDropdown.ClearOptions();
        var options = new List<string>();

        int currentIndex = 0;
        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            var r = uniqueResolutions[i];
            options.Add($"{r.width} x {r.height}");

            if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);

        // ---- Load saved ----
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.7f);
        float sfxVol   = PlayerPrefs.GetFloat("SFXVol", 0.7f);

        int savedResIndex = PlayerPrefs.GetInt("ResIndex", currentIndex);
        savedResIndex = Mathf.Clamp(savedResIndex, 0, uniqueResolutions.Count - 1);

        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        // Set UI values without triggering callbacks
        musicSlider.SetValueWithoutNotify(musicVol);
        sfxSlider.SetValueWithoutNotify(sfxVol);
        resolutionDropdown.SetValueWithoutNotify(savedResIndex);
        fullscreenToggle.SetIsOnWithoutNotify(fullscreen);

        resolutionDropdown.RefreshShownValue();

        // ---- Apply once ----
        ApplyAudio(musicVol, sfxVol);
        ApplyDisplay(savedResIndex, fullscreen);

        isInitializing = false;
    }

    // Hook these from UI events (Inspector)
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

    public void OnResolutionChanged(int index)
    {
        if (isInitializing) return;
        ApplyDisplay(index, fullscreenToggle.isOn);
    }

    public void OnFullscreenChanged(bool on)
    {
        if (isInitializing) return;
        ApplyDisplay(resolutionDropdown.value, on);
    }

    private void ApplyAudio(float musicVol, float sfxVol)
    {
        ApplyMusic(musicVol);
        ApplySfx(sfxVol);
    }

    private void ApplyMusic(float v)
    {
        if (musicSource != null) musicSource.volume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("MusicVol", v);
        PlayerPrefs.Save();
    }

    private void ApplySfx(float v)
    {
        if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("SFXVol", v);
        PlayerPrefs.Save();
    }

    private void ApplyDisplay(int resIndex, bool fullscreen)
    {
        resIndex = Mathf.Clamp(resIndex, 0, uniqueResolutions.Count - 1);

        var r = uniqueResolutions[resIndex];

        // Fullscreen behavior that actually toggles back on reliably
        Screen.fullScreenMode = fullscreen
            ? FullScreenMode.FullScreenWindow    // or ExclusiveFullScreen if you prefer
            : FullScreenMode.Windowed;

        Screen.SetResolution(r.width, r.height, Screen.fullScreenMode);

        PlayerPrefs.SetInt("ResIndex", resIndex);
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
