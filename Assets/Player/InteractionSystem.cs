using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Activates InteractiveObjects based on player input. This component needs to be placed on the
/// same game objects as a collider so it receives the OnTriggerStay2D message.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    private InputAction _interactAction;
    private List<InteractiveObject> _relevantObjects = new();
    private InteractiveObject _lastActiveObject = null;

    void Start()
    {
        _interactAction = InputSystem.actions.FindAction("Interact");
    }

    void Update()
    {
        var playerPosition = transform.position;
        // Find the closest interactive object, that one will be interacted with -- if no objects are close enough, this returns null
        var activeObject = _relevantObjects.OrderBy(v => Vector3.SqrMagnitude(v.transform.position - playerPosition)).FirstOrDefault();

        // Trigger events on the objects so they can show or hide their prompts
        if (activeObject != _lastActiveObject)
        {
            if (_lastActiveObject != null) _lastActiveObject.OnDeactivate.Invoke();
            if (activeObject != null) activeObject.OnActivate.Invoke();
        }

        _lastActiveObject = activeObject;

        if (_interactAction.WasPressedThisFrame())
        {
            if (activeObject != null) activeObject.OnInteract.Invoke();
        }
    }

    void FixedUpdate()
    {
        _relevantObjects.Clear();
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<InteractiveObject>(out var interactive))
        {
            _relevantObjects.Add(interactive);
        }
    }
}
