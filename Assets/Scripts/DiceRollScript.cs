using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DiceRollScript : MonoBehaviour
{
    public event Action<int> OnDiceLanded;

    Rigidbody rBody;
    Vector3 startPosition;

    [Header("Roll Force")]
    [SerializeField] float maxRandForcVal = 25f;
    [SerializeField] float startRollingForce = 1000f;

    [Header("Landing Detection")]
    [SerializeField] float settleVelocity = 0.08f;   // smaller = stricter
    [SerializeField] float settleAngular = 0.08f;    // smaller = stricter
    [SerializeField] float settleTime = 1.0f;        // seconds under threshold

    float settleTimer = 0f;

    public int LastValue { get; private set; } = 0;
    public bool isLanded = false;
    public bool firstThrow = false;

    DiceBottomDetector bottomDetector;

    void Awake()
    {
        startPosition = transform.position;
        rBody = GetComponent<Rigidbody>();
        bottomDetector = GetComponent<DiceBottomDetector>(); // needs this component on same Dice object
        ResetDice();
    }

    void Initialize()
    {
        rBody.isKinematic = true;
        transform.rotation = UnityEngine.Random.rotation;
    }

    void RollDice()
    {
        isLanded = false;
        settleTimer = 0f;

        rBody.isKinematic = false;

        float forceX = UnityEngine.Random.Range(0, maxRandForcVal);
        float forceY = UnityEngine.Random.Range(0, maxRandForcVal);
        float forceZ = UnityEngine.Random.Range(0, maxRandForcVal);

        rBody.AddForce(Vector3.up * UnityEngine.Random.Range(800f, startRollingForce));
        rBody.AddTorque(forceX, forceY, forceZ, ForceMode.Impulse);
    }

    public void ResetDice()
    {
        transform.position = startPosition;

        firstThrow = false;
        isLanded = false;
        settleTimer = 0f;
        LastValue = 0;

        Initialize();
    }

    void Update()
    {
        // Click dice to roll
        if ((Input.GetMouseButtonDown(0) && isLanded) ||
            (Input.GetMouseButtonDown(0) && !firstThrow))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    firstThrow = true;
                    RollDice();
                }
            }
        }

        // Landing check
        if (!rBody.isKinematic && !isLanded)
        {
            bool slow =
                rBody.linearVelocity.magnitude < settleVelocity &&
                rBody.angularVelocity.magnitude < settleAngular;

            if (slow) settleTimer += Time.deltaTime;
            else settleTimer = 0f;

            if (settleTimer >= settleTime)
            {
                isLanded = true;
                rBody.isKinematic = true;

                // Read bottom value from triggers, convert to top.
                int top = 0;
                if (bottomDetector != null)
                    top = bottomDetector.GetTopValue();

                // Fallback if detector not ready
                LastValue = (top >= 1 && top <= 6) ? top : UnityEngine.Random.Range(1, 7);

                OnDiceLanded?.Invoke(LastValue);
            }
        }
    }
}
