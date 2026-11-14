using UnityEngine;

/// <summary>
/// The jar the sorted buttons go into. A button enters through the mouth and then falls and
/// piles up inside, held in by the edge collider that follows the glass.
/// </summary>
public class ButtonJar : MonoBehaviour
{
    public Collider2D DropZone;
    public Transform Mouth;
    public float MouthWidth = 0.6f;
    public float DropDuration = 0.25f;

    public float ButtonScale = 0.33f;
    // How much room a button takes up in the pile, below 1 the buttons overlap a little
    public float Packing = 0.92f;
    public int SortingOrder = -60;
    public Color GlassTint = new Color(0.84f, 0.93f, 0.95f);

    public bool Contains(Vector2 point)
    {
        return DropZone.OverlapPoint(point);
    }
}
