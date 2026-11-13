using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Lets the player pick up a button with the mouse and carry it around. This only moves the
/// button, what happens to it after it is released is up to the listeners.
/// </summary>
public class ButtonDragger : MonoBehaviour
{
    public UnityEvent<SortingButton> OnPickUp;
    public UnityEvent<SortingButton> OnRelease;

    private InputAction _click;
    private InputAction _point;
    private SortingButton _held;
    private Vector3 _grabOffset;
    private bool _wasPressed;

    void Awake()
    {
        _click = InputSystem.actions.FindAction("UI/Click", throwIfNotFound: true);
        _point = InputSystem.actions.FindAction("UI/Point", throwIfNotFound: true);
    }

    void Update()
    {
        var pointer = Camera.main.ScreenToWorldPoint(_point.ReadValue<Vector2>());
        pointer.z = 0.0f;

        var isPressed = _click.IsPressed();
        if (isPressed && !_wasPressed) PickUp(pointer);
        if (!isPressed && _held != null) Release();
        _wasPressed = isPressed;

        if (_held != null)
        {
            _held.transform.position = KeepOnScreen(pointer + _grabOffset);
        }
    }

    void OnDisable()
    {
        if (_held == null) return;

        _held.PutDown();
        _held = null;
    }

    private void PickUp(Vector3 pointer)
    {
        // Buttons can lie on top of each other, take the one the player actually sees
        var button = Physics2D.OverlapPointAll(pointer)
            .Select(v => v.GetComponent<SortingButton>())
            .Where(v => v != null && !v.IsMoving)
            .OrderByDescending(v => v.SortingOrder)
            .FirstOrDefault();

        if (button == null) return;

        _held = button;
        // Hold the button where it was grabbed instead of snapping its center to the cursor
        _grabOffset = button.transform.position - pointer;
        button.PickUp();
        OnPickUp.Invoke(button);
    }

    private void Release()
    {
        var button = _held;
        _held = null;
        button.PutDown();
        OnRelease.Invoke(button);
    }

    private static Vector3 KeepOnScreen(Vector3 position)
    {
        var min = Camera.main.ViewportToWorldPoint(Vector3.zero);
        var max = Camera.main.ViewportToWorldPoint(Vector3.one);

        return new Vector3(
            Mathf.Clamp(position.x, min.x, max.x),
            Mathf.Clamp(position.y, min.y, max.y),
            position.z
        );
    }
}
