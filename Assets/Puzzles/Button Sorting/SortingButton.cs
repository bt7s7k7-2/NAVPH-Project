using System.Collections;
using UnityEngine;

/// <summary>
/// A single button of the sorting puzzle. Knows how many holes it has and animates its own
/// movement, both on the table and when it is dropped into the jar.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public class SortingButton : MonoBehaviour
{
    public int Holes;
    public float HeldScale = 1.15f;
    public float TrembleAngle = 14.0f;
    public float TrembleSpeed = 45.0f;
    public Color StrainColor = new Color(1.0f, 0.55f, 0.55f);

    public bool IsMoving => _movement != null;
    public bool IsInJar { get; private set; }

    public int SortingOrder
    {
        get => _renderer.sortingOrder;
        set => _renderer.sortingOrder = value;
    }

    private SpriteRenderer _renderer;
    private CircleCollider2D _collider;
    private Rigidbody2D _body;
    private Vector3 _restScale;
    private Quaternion _restRotation;
    private Coroutine _movement;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<CircleCollider2D>();
        _body = GetComponent<Rigidbody2D>();
        _restScale = transform.localScale;
    }

    public void Setup(Sprite sprite, int holes)
    {
        Holes = holes;
        _renderer.sprite = sprite;
        // Keep the clickable area the same size as the artwork
        _collider.radius = sprite.bounds.extents.x;
    }

    public void PickUp()
    {
        _restRotation = transform.rotation;
        transform.localScale = _restScale * HeldScale;
    }

    public void PutDown()
    {
        transform.rotation = _restRotation;
        transform.localScale = _restScale;
        _renderer.color = Color.white;
    }

    /// <summary>
    /// Shows how close the held button is to slipping out of the hand, 0 is a firm grip and
    /// 1 is the moment it falls. The button trembles and reddens as a warning to slow down.
    /// </summary>
    public void ShowStrain(float strain)
    {
        var angle = Mathf.Sin(Time.time * TrembleSpeed) * TrembleAngle * strain;
        transform.rotation = _restRotation * Quaternion.Euler(0.0f, 0.0f, angle);
        _renderer.color = Color.Lerp(Color.white, StrainColor, strain);
    }

    public void SlideTo(Vector3 position, float duration)
    {
        StartMovement(position, _restScale, duration, null);
    }

    public void DropInto(ButtonJar jar)
    {
        IsInJar = true;

        // Each button enters at a slightly different spot so they do not stack up in one column
        var entry = jar.Mouth.position + Vector3.right * Random.Range(-0.5f, 0.5f) * jar.MouthWidth;
        StartMovement(entry, Vector3.one * jar.ButtonScale, jar.DropDuration, () =>
        {
            SortingOrder = jar.SortingOrder;
            _renderer.color = jar.GlassTint;

            // From here on physics takes over, the button falls and piles up with the others
            _collider.radius *= jar.Packing;
            _collider.isTrigger = false;
            _body.bodyType = RigidbodyType2D.Dynamic;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;
        });
    }

    private void StartMovement(Vector3 position, Vector3 scale, float duration, System.Action onArrival)
    {
        if (_movement != null) StopCoroutine(_movement);
        _movement = StartCoroutine(Move(position, scale, duration, onArrival));
    }

    private IEnumerator Move(Vector3 targetPosition, Vector3 targetScale, float duration, System.Action onArrival)
    {
        var startPosition = transform.position;
        var startScale = transform.localScale;

        for (var time = 0.0f; time < duration; time += Time.deltaTime)
        {
            // Start fast and slow down towards the end
            var t = 1.0f - Mathf.Pow(1.0f - time / duration, 3.0f);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = targetScale;
        _movement = null;

        onArrival?.Invoke();
    }
}
