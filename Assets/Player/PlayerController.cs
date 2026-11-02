using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float Speed = 1.0f;
    public InputActionAsset InputActions;
    private Rigidbody2D _rigidbody;
    private InputAction _moveAction;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _moveAction = InputSystem.actions.FindAction("Move", throwIfNotFound: true);
    }

    void FixedUpdate()
    {
        var vector = _moveAction.ReadValue<Vector2>();
        _rigidbody.linearVelocity = vector * Speed;
    }
}
