using System.Collections;
using UnityEngine;

/// <summary>
/// A single button lying on the table. Knows how many holes it has and animates its own movement.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
public class SortingButton : MonoBehaviour
{
    public int Holes;
    public float HeldScale = 1.15f;

    public bool IsMoving => _movement != null;

    public int SortingOrder
    {
        get => _renderer.sortingOrder;
        set => _renderer.sortingOrder = value;
    }

    private SpriteRenderer _renderer;
    private Vector3 _restScale;
    private Coroutine _movement;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _restScale = transform.localScale;
    }

    public void Setup(Sprite sprite, int holes)
    {
        Holes = holes;
        _renderer.sprite = sprite;
        // Keep the clickable area the same size as the artwork
        GetComponent<CircleCollider2D>().radius = sprite.bounds.extents.x;
    }

    public void PickUp()
    {
        transform.localScale = _restScale * HeldScale;
    }

    public void PutDown()
    {
        transform.localScale = _restScale;
    }

    public void SlideTo(Vector3 position, float duration)
    {
        StartMovement(position, _restScale, duration, hideWhenDone: false);
    }

    public void SinkInto(Vector3 position, float duration)
    {
        StartMovement(position, Vector3.zero, duration, hideWhenDone: true);
    }

    private void StartMovement(Vector3 position, Vector3 scale, float duration, bool hideWhenDone)
    {
        if (_movement != null) StopCoroutine(_movement);
        _movement = StartCoroutine(Move(position, scale, duration, hideWhenDone));
    }

    private IEnumerator Move(Vector3 targetPosition, Vector3 targetScale, float duration, bool hideWhenDone)
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

        if (hideWhenDone) gameObject.SetActive(false);
    }
}
