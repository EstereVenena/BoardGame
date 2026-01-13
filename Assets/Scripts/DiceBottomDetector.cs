using UnityEngine;

public class DiceBottomDetector : MonoBehaviour
{
    Transform[] facePoints = new Transform[6];

    void Awake()
    {
        AutoFindFaces();
    }

    void AutoFindFaces()
    {
        for (int i = 1; i <= 6; i++)
        {
            Transform found = FindChildRecursive(transform, i.ToString());
            if (found == null)
                Debug.LogError($"DiceBottomDetector: Could NOT find face '{i}' anywhere under Dice!");
            facePoints[i - 1] = found;
        }
    }

    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildRecursive(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    public int GetBottomValue()
    {
        Vector3 center = transform.position;

        int bestValue = 0;
        float bestScore = -999f;

        for (int i = 0; i < 6; i++)
        {
            Transform t = facePoints[i];
            if (t == null) continue;

            Vector3 dir = (t.position - center).normalized;
            float score = Vector3.Dot(dir, Vector3.down);

            if (score > bestScore)
            {
                bestScore = score;
                bestValue = i + 1;
            }
        }

        return bestValue;
    }

    public int GetTopValue()
    {
        int bottom = GetBottomValue();
        if (bottom < 1 || bottom > 6) return 0;
        return 7 - bottom;
    }
}
