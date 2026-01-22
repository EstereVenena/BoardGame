using UnityEngine;

public class AnimTimeProbe : MonoBehaviour
{
    Animator a;
    void Awake() => a = GetComponent<Animator>();

    void Update()
    {
        var st = a.GetCurrentAnimatorStateInfo(0);
        Debug.Log("normalizedTime=" + st.normalizedTime);
    }
}
