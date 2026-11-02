using UnityEngine;

public class PlayerCameraFollow : MonoBehaviour
{
    public Transform FollowTarget;
    public float MaxDistance = 1.0f;
    public RigidbodyConstraints2D Constraints;

    void LateUpdate()
    {
        var originalPosition = transform.position;
        var error = originalPosition - FollowTarget.position;
        var resultPosition = FollowTarget.position + Vector3.ClampMagnitude(error, MaxDistance);

        if (Constraints.HasFlag(RigidbodyConstraints2D.FreezePositionX)) resultPosition.x = originalPosition.x;
        if (Constraints.HasFlag(RigidbodyConstraints2D.FreezePositionY)) resultPosition.y = originalPosition.y;

        transform.position = resultPosition;
    }
}
