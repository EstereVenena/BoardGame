using UnityEngine;

public class DiceAnimatorScript : MonoBehaviour
{
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void RollDice()
    {
        animator.SetBool("isRolling", true);
    }

    public void StopDice()
    {
        animator.SetBool("isRolling", false);
    }
}
