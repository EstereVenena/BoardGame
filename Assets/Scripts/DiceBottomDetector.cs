using UnityEngine;

public class DiceBottomDetector : MonoBehaviour
{
    [Header("Assign face point transforms in order 1..6")]
    public Transform[] facePoints = new Transform[6];

    public int GetBottomValue()
    {
        if (facePoints == null || facePoints.Length != 6)
            return 0;

        int bestValue = 0;
        float lowestY = float.PositiveInfinity;

        for (int i = 0; i < facePoints.Length; i++)
        {
            Transform t = facePoints[i];
            if (t == null) continue;

            float y = t.position.y;
            if (y < lowestY)
            {
                lowestY = y;
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
