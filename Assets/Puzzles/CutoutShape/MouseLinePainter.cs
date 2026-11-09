using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLinePainter : MonoBehaviour
{
    public LineRenderer Renderer;
    public float MinDistance = 0.1f;
    public Vector2 ScreenSpaceOffset = Vector2.zero;
    public Vector2 WorldSpaceOffset = new Vector2(0.5f, 0.0f);

    protected Vector2? _lastPosition = null;
    protected Vector3[] _points;
    protected int _pointCount = 0;
    protected InputAction _click;

    public void DumpPositions()
    {
        Debug.Log("Positions:\n" + String.Join('\n', _points[.._pointCount].Select(v => $"  - {{ x: {v.x}, y: {v.y}, z: 0 }}")));
    }

    void Awake()
    {
        _points = new Vector3[1024];
        _click = InputSystem.actions.FindAction("Click");
    }

    void Update()
    {
        if (_click.IsPressed())
        {
            var position = transform.worldToLocalMatrix * Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue() + ScreenSpaceOffset);

            position = new Vector3(
                Mathf.Clamp(position.x + WorldSpaceOffset.x, -0.5f, 0.5f),
                Mathf.Clamp(position.y + WorldSpaceOffset.y, -0.5f, 0.5f),
                0.0f
            );

            if (_lastPosition is null && (position.x == -0.5f || position.y == -0.5f || position.x == 0.5f || position.y == 0.5f))
            {
                // Do not start drawing when the mouse is not over the drawing area
                return;
            }

            if (_lastPosition is { } lastPosition)
            {
                _points[_pointCount - 1] = position;

                if (Vector3.Distance(lastPosition, position) > MinDistance)
                {
                    _points[_pointCount++] = position;
                    _lastPosition = position;
                }
            }
            else
            {
                _pointCount = 0;
                _points[0] = position;
                _points[1] = position;
                _pointCount = 2;
                _lastPosition = position;
            }

            Renderer.positionCount = _pointCount;
            Renderer.SetPositions(_points);
        }
        else
        {
            _lastPosition = null;
        }
    }
}
