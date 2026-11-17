using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallDetector : MonoBehaviour
{
    [SerializeField] Sprite circleSprite;
    [SerializeField] float horizontalVelocity = 1.4f;
    [SerializeField] float verticalImpulse = 3.5f;
    [SerializeField] Vector2 mazeStartPosition = new Vector2(-3.71f, -2.96f);

    const int CircleCount = 6;
    bool wallHit;
    List<CircleCollider2D> spawnedColliders;
    CircleCollider2D blackTarget;
    Rigidbody2D draggedBody;
    Vector3 dragOffset;

    void Start()
    {
        ResetToMazeStart();
    }

    void ResetToMazeStart()
    {
        transform.position = new Vector3(mazeStartPosition.x, mazeStartPosition.y, 0f);

        Camera camera = Camera.main;
        Mouse mouse = Mouse.current;
        if (camera != null && mouse != null)
            mouse.WarpCursorPosition(camera.WorldToScreenPoint(transform.position));

        Cursor.visible = false;
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null || spawnedColliders == null)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 pointer = camera.ScreenToWorldPoint(new Vector3(
            mouse.position.ReadValue().x,
            mouse.position.ReadValue().y,
            -camera.transform.position.z));
        pointer.z = 0f;

        if (draggedBody == null && mouse.leftButton.wasPressedThisFrame)
        {
            foreach (Collider2D hit in Physics2D.OverlapPointAll(pointer))
            {
                CircleCollider2D collider = hit.GetComponent<CircleCollider2D>();
                if (!spawnedColliders.Contains(collider))
                    continue;

                draggedBody = collider.attachedRigidbody;
                dragOffset = draggedBody.position - (Vector2)pointer;
                draggedBody.linearVelocity = Vector2.zero;
                draggedBody.bodyType = RigidbodyType2D.Kinematic;
                break;
            }
        }

        if (draggedBody == null)
            return;

        if (mouse.leftButton.isPressed)
        {
            draggedBody.position = (Vector2)pointer + (Vector2)dragOffset;
            if (blackTarget != null && blackTarget.OverlapPoint(draggedBody.position))
                CollectDraggedCircle();
        }
        else
        {
            draggedBody.bodyType = RigidbodyType2D.Dynamic;
            draggedBody = null;
        }
    }

    void CollectDraggedCircle()
    {
        CircleCollider2D collectedCollider = draggedBody.GetComponent<CircleCollider2D>();
        spawnedColliders.Remove(collectedCollider);
        Destroy(draggedBody.gameObject);
        draggedBody = null;

        if (spawnedColliders.Count == 0)
            CompleteGutsCollection();
    }

    void CompleteGutsCollection()
    {
        Destroy(blackTarget.gameObject);
        blackTarget = null;
        spawnedColliders = null;
        wallHit = false;
        ResetToMazeStart();

        if (TryGetComponent<FollowCursor>(out var followCursor))
            followCursor.enabled = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        DetectWall(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        DetectWall(other);
    }

    void DetectWall(Collider2D other)
    {
        if (!other.CompareTag("Wall") && other.GetComponent<EdgeCollider2D>() == null)
            return;

        if (wallHit)
            return;

        wallHit = true;
        HandleWallHit();
    }

    void HandleWallHit()
    {
        Debug.Log("Hit a wall!");

        if (TryGetComponent<FollowCursor>(out var followCursor))
            followCursor.enabled = false;

        Cursor.visible = true;
        SpawnCircles();
    }

    void SpawnCircles()
    {
        Camera camera = Camera.main;
        Sprite burstSprite = circleSprite != null
            ? circleSprite
            : GetComponent<SpriteRenderer>()?.sprite;
        Vector3 spawnPosition = camera != null
            ? camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -camera.transform.position.z))
            : Vector3.zero;
        spawnPosition.z = 0f;
        CreateFloor(camera);
        CreateBlackTarget(spawnPosition, burstSprite);
        spawnedColliders = new List<CircleCollider2D>(CircleCount);

        for (int i = 0; i < CircleCount; i++)
        {
            GameObject circle = new GameObject("Wall hit circle");
            float centeredIndex = i - (CircleCount - 1) / 2f;
            float horizontalOffset = centeredIndex * 0.28f;
            circle.transform.position = spawnPosition + Vector3.right * horizontalOffset;
            circle.transform.localScale = Vector3.one * Random.Range(0.88f, 1.52f);

            SpriteRenderer renderer = circle.AddComponent<SpriteRenderer>();
            renderer.sprite = burstSprite;
            renderer.color = Color.yellow;
            renderer.sortingOrder = 2;

            CircleCollider2D collider = circle.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            Rigidbody2D body = circle.AddComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            body.freezeRotation = true;
            foreach (EdgeCollider2D wall in FindObjectsByType<EdgeCollider2D>())
                Physics2D.IgnoreCollision(collider, wall);
            foreach (CircleCollider2D other in spawnedColliders)
                Physics2D.IgnoreCollision(collider, other);
            spawnedColliders.Add(collider);
            float outwardDirection = Mathf.Sign(centeredIndex);
            float outwardAmount = Mathf.Abs(centeredIndex) / ((CircleCount - 1) / 2f);
            float horizontalSpeed = Random.Range(
                horizontalVelocity * Mathf.Lerp(0.8f, 1f, outwardAmount),
                horizontalVelocity * Mathf.Lerp(1f, 1.2f, outwardAmount));
            body.linearVelocity = new Vector2(
                outwardDirection * horizontalSpeed,
                Random.Range(verticalImpulse * 0.7f, verticalImpulse * 1.3f));

        }
    }

    void CreateBlackTarget(Vector3 spawnPosition, Sprite burstSprite)
    {
        GameObject target = new GameObject("Guts collection target");
        target.transform.position = spawnPosition;
        target.transform.localScale = Vector3.one * 1.2f;

        SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
        renderer.sprite = burstSprite;
        renderer.color = Color.black;
        renderer.sortingOrder = 1;

        blackTarget = target.AddComponent<CircleCollider2D>();
        blackTarget.radius = 0.5f;
        blackTarget.isTrigger = true;
    }

    void CreateFloor(Camera camera)
    {
        GameObject floor = new GameObject("Wall hit floor");
        float floorHeight = 0.5f;
        SpriteRenderer playfield = GameObject.Find("Test zipper_0")?.GetComponent<SpriteRenderer>();
        float centerX = camera != null
            ? camera.transform.position.x
            : playfield != null
                ? playfield.bounds.center.x
                : 0f;
        float floorTop = playfield != null
            ? playfield.bounds.min.y
            : camera != null
                ? camera.transform.position.y - camera.orthographicSize + 0.25f
                : -4.75f;
        float width = camera != null ? camera.orthographicSize * camera.aspect * 2f : 20f;
        floor.transform.position = new Vector3(centerX, floorTop - floorHeight / 2f, 0f);

        BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(width, floorHeight);

        CreateSideBoundary("Wall hit left boundary", centerX - width / 2f, floorTop, floorHeight, width);
        CreateSideBoundary("Wall hit right boundary", centerX + width / 2f, floorTop, floorHeight, width);
    }

    void CreateSideBoundary(string name, float x, float floorTop, float floorHeight, float width)
    {
        GameObject boundary = new GameObject(name);
        boundary.transform.position = new Vector3(x, floorTop + 1f, 0f);

        BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(floorHeight, width);
    }
}
