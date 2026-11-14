using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Lets the player pick up a button with the mouse and carry it around. This only moves the
/// button, what happens to it after it is released or dropped is up to the listeners.
/// </summary>
public class ButtonDragger : MonoBehaviour
{
    public bool Slippery = true;
    public float SafeSpeed = 12.0f;
    // How long the button holds on when carried at twice the safe speed, faster means shorter
    public float GripTime = 0.2f;
    public float RecoveryTime = 0.4f;

    public UnityEvent<SortingButton> OnPickUp;
    public UnityEvent<SortingButton> OnRelease;
    public UnityEvent<SortingButton, Vector2> OnSlip;

    private InputAction _click;
    private InputAction _point;
    private SortingButton _held;
    private Vector3 _grabOffset;
    private bool _wasPressed;
    private Vector2 _velocity;
    private float _strain;

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

        if (_held == null) return;

        var target = KeepOnScreen(pointer + _grabOffset);
        var velocity = (target - _held.transform.position) / Time.deltaTime;
        _held.transform.position = target;

        if (Slippery) UpdateGrip(velocity);
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
            .Where(v => v != null && !v.IsMoving && !v.IsInJar)
            .OrderByDescending(v => v.SortingOrder)
            .FirstOrDefault();

        if (button == null) return;

        _held = button;
        // Hold the button where it was grabbed instead of snapping its center to the cursor
        _grabOffset = button.transform.position - pointer;
        _velocity = Vector2.zero;
        _strain = 0.0f;
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

    // Carrying a button faster than the safe speed wears the grip down until the button slips.
    // Slowing down lets the grip recover.
    private void UpdateGrip(Vector2 velocity)
    {
        // Smoothed so a single jumpy frame does not count as a fast movement
        _velocity = Vector2.Lerp(_velocity, velocity, 1.0f - Mathf.Exp(-Time.deltaTime / 0.05f));

        var excess = _velocity.magnitude / SafeSpeed - 1.0f;
        var change = excess > 0.0f ? excess / GripTime : -1.0f / RecoveryTime;
        _strain = Mathf.Clamp01(_strain + change * Time.deltaTime);
        _held.ShowStrain(_strain);

        if (_strain < 1.0f) return;

        var button = _held;
        _held = null;
        button.PutDown();
        OnSlip.Invoke(button, _velocity);
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
