using UnityEngine;

public class DiceFaceTrigger : MonoBehaviour
{
    DiceBottomDetector detector;

    void Awake()
    {
        detector = GetComponentInParent<DiceBottomDetector>();
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Ground")) return;

        int v;
        if (int.TryParse(gameObject.name, out v))
            detector.SetBottom(v);
    }
}
