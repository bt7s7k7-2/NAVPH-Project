using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Marks an object as interactive. Expected is a collider component on the same game object, with
/// isTrigger enabled. That trigger will be detected by InteractionSystem which will call our
/// events.
/// </summary>
public class InteractiveObject : MonoBehaviour
{
    public UnityEvent OnActivate;
    public UnityEvent OnDeactivate;
    public UnityEvent OnInteract;
}
