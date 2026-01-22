using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomIdlePicker : MonoBehaviour
{
    [Header("Idle Randomization")]
    [Tooltip("Number of idle states (IdleIndex = 0..idleCount-1)")]
    public int idleCount = 4;

    [Tooltip("Animator int parameter name (must match Animator Parameters)")]
    public string paramName = "IdleIndex";

    [Header("Optional Auto-Change")]
    [Tooltip("0 = disabled. If > 0, pick a new idle every X seconds (unscaled).")]
    public float changeEverySeconds = 0f;

    private Animator anim;

    // Bag of remaining indices (no repeats until bag refills)
    private readonly List<int> bag = new List<int>();
    private int lastPlayed = -1;
    private float timer = 0f;

    void Awake()
    {
        anim = GetComponent<Animator>();

        // Make UI animators immune to pause/timeScale
        anim.enabled = true;
        anim.speed = 1f;
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        RefillBag();
    }

    void OnEnable()
    {
        // Reset timer so it doesn't instantly flip
        timer = 0f;

        // Pick an idle when shown
        PickNextIdle();
    }

    void Update()
    {
        if (changeEverySeconds <= 0f) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= changeEverySeconds)
        {
            timer = 0f;
            PickNextIdle();
        }
    }

    /// <summary>
    /// New name: picks the next idle using a shuffle-bag (no repeats until all used).
    /// </summary>
    public void PickNextIdle()
    {
        if (!anim || idleCount <= 0) return;

        if (bag.Count == 0)
            RefillBag();

        // Random pick from bag
        int pickAt = Random.Range(0, bag.Count);
        int idleIndex = bag[pickAt];

        // Avoid immediate repeat (especially across refills)
        if (idleIndex == lastPlayed && bag.Count > 1)
        {
            int alt = (pickAt + 1) % bag.Count;
            idleIndex = bag[alt];
            pickAt = alt;
        }

        // Remove so it can't be repeated until refill
        bag.RemoveAt(pickAt);

        // Apply to Animator
        anim.SetInteger(paramName, idleIndex);

        // Force animator to evaluate transitions immediately
        anim.Update(0f);

        // Randomize start time inside the current state (prevents sync)
        var st = anim.GetCurrentAnimatorStateInfo(0);
        anim.Play(st.fullPathHash, 0, Random.value);

        lastPlayed = idleIndex;
    }

    /// <summary>
    /// Backwards compatible name (your older code calls this).
    /// </summary>
    public void PickRandomIdle()
    {
        PickNextIdle();
    }

    private void RefillBag()
    {
        bag.Clear();
        for (int i = 0; i < idleCount; i++)
            bag.Add(i);

        // Shuffle the bag
        for (int i = 0; i < bag.Count; i++)
        {
            int j = Random.Range(i, bag.Count);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }

        // Ensure first pick after refill isn't the same as lastPlayed
        if (bag.Count > 1 && bag[0] == lastPlayed)
        {
            int swapWith = Random.Range(1, bag.Count);
            (bag[0], bag[swapWith]) = (bag[swapWith], bag[0]);
        }
    }
}
