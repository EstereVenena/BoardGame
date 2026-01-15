using UnityEngine;
using TMPro;

public class NameScript : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;

    void Awake()
    {
        if (nameText == null)
        {
            nameText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    public void SetName(string name)
    {
        if (nameText == null) return;

        nameText.text = name;
        nameText.color = new Color32(
            (byte)Random.Range(0, 256),
            (byte)Random.Range(0, 256),
            (byte)Random.Range(0, 256),
            255);
    }
}
