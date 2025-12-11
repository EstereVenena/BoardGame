using UnityEngine;
using UnityEngine.UI;

public class RolledNumberScript : MonoBehaviour
{
    DiceRollScript diceRollScript;
    [SerializeField]
    Text rolledNumberText;


    void Awake()
    {
        diceRollScript = FindFirstObjectByType<DiceRollScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (diceRollScript != null)
        {
            if (diceRollScript.isLanded)
                rolledNumberText.text = diceRollScript.diceFaceNum;

            else
                rolledNumberText.text = "?";

        }
        else
            Debug.LogWarning("DiceRollScript not found!");
    }
}
