using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    public Transform player;
    public float distance = 10f;
    public float height = 5f;
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 desiredPosition = player.position + new Vector3(0, height, distance);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        transform.LookAt(player.position + Vector3.up * (height * 0.5f));
    }
}
