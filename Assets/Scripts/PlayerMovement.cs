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
        rb.MovePosition(rb.position + delta);
    }
}
