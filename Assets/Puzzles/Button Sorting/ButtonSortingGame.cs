using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Runs the button sorting puzzle. The buttons get spilled onto the table and the player has to
/// put them back into the jar, leaving out the ones with the unwanted number of holes.
/// </summary>
public class ButtonSortingGame : MonoBehaviour
{
    [System.Serializable]
    public class ButtonKind
    {
        public Sprite Sprite;
        public int Holes;
        public int Count = 1;
    }

    public SortingButton ButtonPrefab;
    public ButtonDragger Dragger;
    public CountdownTimer Timer;
    public Collider2D Table;
    public Collider2D Jar;
    public TMP_Text ProgressLabel;

    public List<ButtonKind> ButtonKinds;
    public int UnwantedHoleCount = 2;
    public bool RandomRotation = true;
    public float SpillDuration = 0.6f;
    public float DropDuration = 0.25f;

    public UnityEvent OnWrongButton;
    public UnityEvent OnSolved;

    private int _wantedTotal;
    private int _wantedInJar;
    private int _nextSortingOrder;
    private Vector3 _pickUpPosition;

    void Start()
    {
        Dragger.OnPickUp.AddListener(HandlePickUp);
        Dragger.OnRelease.AddListener(HandleRelease);

        SpillButtons();
        UpdateProgress();
        Timer.Run();
    }

    private void SpillButtons()
    {
        var takenSpots = new List<Vector3>();

        foreach (var kind in ButtonKinds)
        {
            for (var i = 0; i < kind.Count; i++)
            {
                var spot = FindFreeSpot(takenSpots);
                takenSpots.Add(spot);

                var rotation = RandomRotation ? Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)) : Quaternion.identity;
                var button = Instantiate(ButtonPrefab, Table.bounds.center, rotation, transform);
                button.Setup(kind.Sprite, kind.Holes);
                button.SortingOrder = _nextSortingOrder++;

                // Buttons grow out of the middle of the table as they slide to their spot
                button.transform.localScale = Vector3.zero;
                button.SlideTo(spot, SpillDuration);

                if (IsWanted(button)) _wantedTotal++;
            }
        }
    }

    // Tries a few random spots on the table and keeps the one farthest from the other buttons,
    // which spreads them out evenly without needing a grid
    private Vector3 FindFreeSpot(List<Vector3> takenSpots)
    {
        var area = Table.bounds;
        var bestSpot = area.center;
        var bestDistance = -1.0f;

        for (var attempt = 0; attempt < 20; attempt++)
        {
            var spot = new Vector3(Random.Range(area.min.x, area.max.x), Random.Range(area.min.y, area.max.y));
            var distance = takenSpots.Select(v => Vector3.Distance(v, spot)).DefaultIfEmpty(0.0f).Min();
            if (distance <= bestDistance) continue;

            bestSpot = spot;
            bestDistance = distance;
        }

        return bestSpot;
    }

    private bool IsWanted(SortingButton button)
    {
        return button.Holes != UnwantedHoleCount;
    }

    private void HandlePickUp(SortingButton button)
    {
        _pickUpPosition = button.transform.position;
        // The held button is always drawn above the rest
        button.SortingOrder = _nextSortingOrder++;
    }

    private void HandleRelease(SortingButton button)
    {
        Vector2 position = button.transform.position;

        if (!Jar.OverlapPoint(position))
        {
            // Buttons can only lie on the table, one dropped anywhere else slides back onto it
            if (!Table.OverlapPoint(position)) button.SlideTo(Table.ClosestPoint(position), DropDuration);
            return;
        }

        if (!IsWanted(button))
        {
            // The jar spits the button back to where it was taken from
            button.SlideTo(_pickUpPosition, DropDuration);
            OnWrongButton.Invoke();
            return;
        }

        button.SinkInto(Jar.transform.position, DropDuration);
        _wantedInJar++;
        UpdateProgress();

        if (_wantedInJar < _wantedTotal) return;

        Timer.Stop();
        Dragger.enabled = false;
        OnSolved.Invoke();
    }

    private void UpdateProgress()
    {
        ProgressLabel.text = $"{_wantedInJar} / {_wantedTotal}";
    }
}
