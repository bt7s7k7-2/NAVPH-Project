using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [SerializeField] Sprite circleSprite;
    [SerializeField] float horizontalImpulse = 10f;
    [SerializeField] float verticalImpulse = 6f;

    const int CircleCount = 6;
    bool wallHit;

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

        for (int i = 0; i < CircleCount; i++)
        {
            GameObject circle = new GameObject("Wall hit circle");
            circle.transform.position = spawnPosition;
            circle.transform.localScale = Vector3.one * Random.Range(0.88f, 1.52f);

            SpriteRenderer renderer = circle.AddComponent<SpriteRenderer>();
            renderer.sprite = burstSprite;
            renderer.color = Color.yellow;
            renderer.sortingOrder = 2;

            CircleCollider2D collider = circle.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            foreach (EdgeCollider2D wall in FindObjectsByType<EdgeCollider2D>())
                Physics2D.IgnoreCollision(collider, wall);

            Rigidbody2D body = circle.AddComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            body.freezeRotation = true;
            body.AddForce(
                new Vector2(
                    (i % 2 == 0 ? -1f : 1f) * Random.Range(horizontalImpulse * 0.7f, horizontalImpulse * 1.3f),
                    Random.Range(verticalImpulse * 0.7f, verticalImpulse * 1.3f)),
                ForceMode2D.Impulse);

        }
    }

    void CreateFloor(Camera camera)
    {
        GameObject floor = new GameObject("Wall hit floor");
        float floorHeight = 0.5f;
        SpriteRenderer playfield = GameObject.Find("Test zipper_0")?.GetComponent<SpriteRenderer>();
        float floorTop = playfield != null
            ? playfield.bounds.min.y
            : camera != null
                ? camera.transform.position.y - camera.orthographicSize + 0.25f
                : -4.75f;
        floor.transform.position = new Vector3(0f, floorTop - floorHeight / 2f, 0f);

        BoxCollider2D collider = floor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(camera != null ? camera.orthographicSize * camera.aspect * 2f + 4f : 20f, floorHeight);
    }
}
