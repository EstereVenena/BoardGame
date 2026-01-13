using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceRollScript : MonoBehaviour
{
    public event Action<int> OnDiceLanded;

    [Header("Roll Settings")]
    [SerializeField] float upwardImpulseMin = 2.0f;
    [SerializeField] float upwardImpulseMax = 4.0f;
    [SerializeField] float sideImpulseMax = 0.6f;
    [SerializeField] float torqueImpulseMax = 6.0f;

    [Header("Reset / Spawn")]
    [SerializeField] float liftOnReset = 0.15f; // lift slightly above ground to avoid overlap pop
    [SerializeField] bool freezeWhileIdle = true;

    [Header("Landing Detection")]
    [SerializeField] float settleLinear = 0.08f;
    [SerializeField] float settleAngular = 0.08f;
    [SerializeField] float settleTime = 1.0f;

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;

    float settleTimer;
    bool sentResult;

    public int LastValue { get; private set; }
    public bool isLanded { get; private set; }
    public bool firstThrow { get; private set; }

    public bool IsLocked { get; private set; }

    DiceBottomDetector bottomDetector;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bottomDetector = GetComponent<DiceBottomDetector>();

        startPos = transform.position;
        startRot = transform.rotation;

        ResetDice();
    }

    public void SetLocked(bool locked) => IsLocked = locked;

    public void ResetDice()
    {
        // Make dynamic so we can safely zero velocities without warnings
        rb.isKinematic = false;

        // Move slightly above the surface to avoid collision “explosion”
        transform.position = startPos + Vector3.up * liftOnReset;
        transform.rotation = startRot;

        // Clear motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Idle state
        firstThrow = false;
        isLanded = true;          // treat reset as "ready"
        settleTimer = 0f;
        sentResult = false;
        LastValue = 0;

        if (freezeWhileIdle)
            rb.isKinematic = true;
    }

    void RollDice()
    {
        // Prepare roll
        isLanded = false;
        settleTimer = 0f;
        sentResult = false;

        // Ensure dynamic
        rb.isKinematic = false;

        // Clear motion so impulse is consistent
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Randomize orientation so it doesn't favor one side
        transform.rotation = UnityEngine.Random.rotation;

        // Add impulses (NOT insane values)
        float up = UnityEngine.Random.Range(upwardImpulseMin, upwardImpulseMax);
        Vector3 side = new Vector3(
            UnityEngine.Random.Range(-sideImpulseMax, sideImpulseMax),
            0f,
            UnityEngine.Random.Range(-sideImpulseMax, sideImpulseMax)
        );

        rb.AddForce((Vector3.up * up) + side, ForceMode.Impulse);

        Vector3 torque = new Vector3(
            UnityEngine.Random.Range(-torqueImpulseMax, torqueImpulseMax),
            UnityEngine.Random.Range(-torqueImpulseMax, torqueImpulseMax),
            UnityEngine.Random.Range(-torqueImpulseMax, torqueImpulseMax)
        );

        rb.AddTorque(torque, ForceMode.Impulse);
    }

    void Update()
    {
        if (IsLocked) return;

        // Click dice to roll
        if (Input.GetMouseButtonDown(0))
        {
            // Only allow roll if landed/ready OR first throw
            if (!isLanded && firstThrow) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    firstThrow = true;
                    RollDice();
                }
            }
        }

        // Landing check (only while moving)
        if (!rb.isKinematic && !isLanded)
        {
            bool slow =
                rb.linearVelocity.magnitude < settleLinear &&
                rb.angularVelocity.magnitude < settleAngular;

            settleTimer = slow ? (settleTimer + Time.deltaTime) : 0f;

            if (settleTimer >= settleTime)
            {
                isLanded = true;

                if (freezeWhileIdle)
                    rb.isKinematic = true;

                if (sentResult) return;
                sentResult = true;

                int top = (bottomDetector != null) ? bottomDetector.GetTopValue() : 0;
                if (top < 1 || top > 6)
                {
                    Debug.LogError("DiceRollScript: failed to detect top face. Check DiceBottomDetector faces.");
                    return;
                }

                LastValue = top;
                OnDiceLanded?.Invoke(LastValue);
            }
        }
    }
}
