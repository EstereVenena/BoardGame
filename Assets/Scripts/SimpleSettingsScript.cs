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

    Resolution[] resolutions;

    void Start()
    {
        // ===== RESOLUTION SETUP =====
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);

            if (resolutions[i].width == Screen.width &&
                resolutions[i].height == Screen.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();

        // ===== LOAD SAVED SETTINGS =====
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.7f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.7f);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        ApplyAll();
    }

    void ApplyAll()
    {
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
        SetFullscreen(fullscreenToggle.isOn);
    }

    public void SetMusicVolume(float v)
    {
        musicSource.volume = v;
        PlayerPrefs.SetFloat("MusicVol", v);
    }

    public void SetSFXVolume(float v)
    {
        sfxSource.volume = v;
        PlayerPrefs.SetFloat("SFXVol", v);
    }

    public void SetResolution(int index)
    {
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool on)
    {
        Screen.fullScreen = on;
        PlayerPrefs.SetInt("Fullscreen", on ? 1 : 0);
    }
}
