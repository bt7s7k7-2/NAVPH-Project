using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float Speed = 1.0f;
    public InputActionAsset InputActions;
    public SpriteRenderer Renderer;
    [SerializeField] public Dictionary<MoveDirection, Sprite> MovementSprites;

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

        if (vector.sqrMagnitude < Mathf.Epsilon)
        {
            // Keep the existing sprite when the character is not moving
            return;
        }

        var direction = Mathf.Abs(vector.x) >= Mathf.Abs(vector.y) ? (
            vector.x > 0 ? MoveDirection.Right : MoveDirection.Left
        ) : (
            vector.y > 0 ? MoveDirection.Up : MoveDirection.Down
        );

        if (MovementSprites.TryGetValue(direction, out var sprite))
        {
            Renderer.sprite = sprite;
        }
    }
}
