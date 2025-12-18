using UnityEngine;

public class SettingsPanelToggle : MonoBehaviour
{
    public GameObject settingsPanel;

    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);
}
