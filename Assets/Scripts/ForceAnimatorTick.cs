using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ForceAnimatorTick : MonoBehaviour
{
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        Apply();
    }

    void OnEnable()
    {
        Apply();
    }

    void Apply()
    {
        if (!anim) return;

        anim.enabled = true;
        anim.speed = 1f;
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // If something keeps resetting it, this helps break the loop
        anim.Rebind();
        anim.Update(0f);
    }
}
