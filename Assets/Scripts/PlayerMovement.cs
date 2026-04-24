using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("Freeze rigidbody rotation so physics doesn't tip the player over.")]
    public bool freezeRotation = true;

    [Header("Audio")]
    [Tooltip("Time between footsteps while moving.")]
    public float footstepInterval = 0.42f;
    float footstepCooldown;

    [Header("Island bounds (no swimming)")]
    [Tooltip("Clamp XZ so the player cannot walk off the island into the ocean.")]
    public bool constrainToIsland = true;
    [Tooltip("Optional: assign an empty at island center. If null, uses world origin.")]
    public Transform islandCenter;
    [Tooltip("Half-width (X) and half-depth (Z) of playable area from center.")]
    public Vector2 islandHalfExtentsXZ = new Vector2(52f, 52f);
    [Tooltip("If Y drops below this (fell in water / off mesh), snap back to safe land height.")]
    public float seaLevelY = 0.35f;
    [Tooltip("Y position used when recovering from sea / void.")]
    public float safeLandY = 1.5f;

    Rigidbody rb;
    Vector3 moveInput;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveInput = new Vector3(h, 0f, v);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        if (footstepInterval > 0f && GameAudio.Instance != null)
        {
            if (moveInput.sqrMagnitude > 0.02f)
            {
                footstepCooldown -= Time.deltaTime;
                if (footstepCooldown <= 0f)
                {
                    GameAudio.Instance.PlayPlayerFootstep();
                    footstepCooldown = footstepInterval;
                }
            }
            else
            {
                footstepCooldown = footstepInterval;
            }
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (freezeRotation)
        {
            rb.freezeRotation = true;
        }
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        Vector3 delta = moveInput * (speed * Time.fixedDeltaTime);
        Vector3 next = rb.position + delta;
        if (constrainToIsland)
            next = ClampToIsland(next);
        rb.MovePosition(next);
    }

    Vector3 ClampToIsland(Vector3 p)
    {
        Vector3 c = islandCenter != null ? islandCenter.position : Vector3.zero;
        p.x = Mathf.Clamp(p.x, c.x - islandHalfExtentsXZ.x, c.x + islandHalfExtentsXZ.x);
        p.z = Mathf.Clamp(p.z, c.z - islandHalfExtentsXZ.y, c.z + islandHalfExtentsXZ.y);

        if (p.y < seaLevelY)
        {
            p.y = safeLandY;
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
        }

        return p;
    }
}
