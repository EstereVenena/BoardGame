using System.Collections;
using UnityEngine;

public class SetActiveButtonScript : MonoBehaviour
{
    [Header("What to toggle")]
    public GameObject targetObject;

    [Tooltip("If true, this object will toggle itself too (your old behavior).")]
    public bool toggleSelfToo = true;

    [Tooltip("Use realtime so it still works when Time.timeScale = 0 (menus/pause).")]
    public bool useRealtime = true;

    Coroutine routine;

    public void ToggleActiveAfterDelay(float delay)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ToggleActiveCoroutine(delay));
    }

    private IEnumerator ToggleActiveCoroutine(float delay)
    {
        if (useRealtime)
            yield return new WaitForSecondsRealtime(delay);
        else
            yield return new WaitForSeconds(delay);

        if (targetObject != null)
            targetObject.SetActive(!targetObject.activeSelf);

        if (toggleSelfToo)
            gameObject.SetActive(!gameObject.activeSelf);

        routine = null;
    }

    // Useful if you want explicit open/close instead of toggle chaos
    public void SetTargetActive(bool on)
    {
        if (targetObject != null) targetObject.SetActive(on);
    }
}
