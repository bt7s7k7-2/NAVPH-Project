using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
        var activeObject = _relevantObjects.OrderBy(v => Vector3.SqrMagnitude(v.transform.position - playerPosition)).FirstOrDefault();

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
