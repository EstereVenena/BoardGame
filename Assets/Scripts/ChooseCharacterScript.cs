using UnityEngine;
using TMPro;

public class ChooseCharacterScript : MonoBehaviour
{
    [Header("Character Selection")]
    public GameObject[] characters;
    private int characterIndex;

    [Header("UI")]
    public TMP_InputField inputField;

    [Header("Players")]
    public int playerCount = 2;

    [Header("Scene")]
    public SceneChanger sceneChanger;

    private void Awake()
    {
        characterIndex = 0;

        // Hide all, then show first
        if (characters != null)
        {
            foreach (GameObject character in characters)
            {
                if (character != null)
                    character.SetActive(false);
            }

            ShowCharacter(characterIndex);
        }
    }

    private void ShowCharacter(int index)
    {
        if (characters == null || characters.Length == 0) return;

        index = Mathf.Clamp(index, 0, characters.Length - 1);

        // Disable all (safe even if already disabled)
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] != null)
                characters[i].SetActive(false);
        }

        // Enable selected
        var go = characters[index];
        if (go == null) return;

        go.SetActive(true);

        // ✅ Randomize idle animation for the newly shown avatar
        var picker = go.GetComponent<RandomIdlePicker>();
        if (picker != null)
            picker.PickRandomIdle();
    }

    public void NextCharacter()
    {
        if (characters == null || characters.Length == 0) return;

        characterIndex++;
        if (characterIndex >= characters.Length)
            characterIndex = 0;

        ShowCharacter(characterIndex);
    }

    public void PreviousCharacter()
    {
        if (characters == null || characters.Length == 0) return;

        characterIndex--;
        if (characterIndex < 0)
            characterIndex = characters.Length - 1;

        ShowCharacter(characterIndex);
    }

    public void Play()
    {
        if (inputField == null)
        {
            Debug.LogError("[ChooseCharacterScript] InputField is not assigned.");
            return;
        }

        string characterName = inputField.text?.Trim() ?? "";

        if (characterName.Length >= 3)
        {
            PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
            PlayerPrefs.SetString("PlayerName", characterName);
            PlayerPrefs.SetInt("PlayerCount", playerCount);
            PlayerPrefs.Save();

            // Build player list now so game scene has correct names
            if (GameSession.I != null)
                GameSession.I.BuildPlayersFromPrefs();

            if (sceneChanger != null)
                StartCoroutine(sceneChanger.Delay("play", characterIndex, characterName));
            else
                Debug.LogError("[ChooseCharacterScript] SceneChanger is not assigned.");
        }
        else
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
}
