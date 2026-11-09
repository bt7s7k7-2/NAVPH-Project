using UnityEngine;
using UnityEngine.Animations;

public class RemoteTransform : MonoBehaviour
{
    public Transform Target;
    public Axis PositionCopy = Axis.X | Axis.Y;
    public Transform SizeReference;
    public Axis ScaleCopy = Axis.None;
    public Vector3 ScaleMultiplier = Vector3.one;

    void Update()
    {
        if (PositionCopy != Axis.None)
        {
            var targetPosition = Target.position;
            var currentPosition = transform.position;

            transform.position = new Vector3(
                PositionCopy.HasFlag(Axis.X) ? targetPosition.x : currentPosition.x,
                PositionCopy.HasFlag(Axis.Y) ? targetPosition.y : currentPosition.y,
                PositionCopy.HasFlag(Axis.Z) ? targetPosition.z : currentPosition.z
            );
        }

        if (ScaleCopy != Axis.None && SizeReference != null)
        {
            var targetPosition = Target.position;
            var targetScale = (SizeReference.position - targetPosition) * 2.0f;
            var currentScale = transform.localScale;

            transform.localScale = new Vector3(
                ScaleCopy.HasFlag(Axis.X) ? Mathf.Abs(targetScale.x) * ScaleMultiplier.x : currentScale.x,
                ScaleCopy.HasFlag(Axis.Y) ? Mathf.Abs(targetScale.y) * ScaleMultiplier.y : currentScale.y,
                ScaleCopy.HasFlag(Axis.Z) ? Mathf.Abs(targetScale.z) * ScaleMultiplier.z : currentScale.z
            );
        }
    }
}
