using UnityEngine;
using UnityEngine.UI;

public class RolledNumberScript : MonoBehaviour
{
    DiceRollScript dice;
    [SerializeField] Text rolledNumberText;

    void Awake()
    {
        dice = FindFirstObjectByType<DiceRollScript>();
    }

    void Update()
    {
        if (dice == null)
        {
            rolledNumberText.text = "?";
            return;
        }

        if (dice.isLanded)
            rolledNumberText.text = dice.LastValue.ToString();
        else
            rolledNumberText.text = "?";
    }
}
