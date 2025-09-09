using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10f);
    public float smoothTime = 0.15f;
    public float maxSpeed = 100f;

    private Vector3 _velocity;

    void LateUpdate()
    {
        if (target == null)
        {
            // Try to find player
            var player = FindObjectOfType<PlayerController>();
            if (player != null) target = player.transform;
            if (target == null) return;
        }

        Vector3 desired = target.position + offset;
        desired.z = offset.z; // lock z
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime, maxSpeed);
    }
}
