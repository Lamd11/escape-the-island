using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("Freeze rigidbody rotation so physics doesn't tip the player over.")]
    public bool freezeRotation = true;

    Rigidbody rb;
    Vector3 moveInput;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveInput = new Vector3(h, 0f, v);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
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
