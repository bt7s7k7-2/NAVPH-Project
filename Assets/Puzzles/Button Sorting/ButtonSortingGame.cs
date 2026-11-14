using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Runs the button sorting puzzle. The buttons get spilled onto the table and the player has to
/// put them back into the jar, leaving out the ones with the unwanted number of holes. Wrong
/// buttons cost time and when the time runs out the puzzle starts over.
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
    public ButtonJar Jar;
    public TMP_Text ProgressLabel;
    public TMP_Text HintLabel;

    public List<ButtonKind> ButtonKinds;
    public int UnwantedHoleCount = 2;
    public bool MessyPile = true;
    // Part of the table the pile covers, a smaller pile hides more of the buttons
    public float PileSize = 0.8f;
    public bool RandomRotation = true;
    public float SpillDuration = 0.6f;
    public float SlideDuration = 0.25f;
    // How far a slipped button flies, in seconds of the speed it was carried at
    public float SlipMomentum = 0.08f;
    public float WrongButtonPenalty = 5.0f;
    public float RestartDelay = 1.5f;

    public float HintDuration = 2.5f;
    public string SlipHint = "Too fast, it slipped!";
    public string WrongButtonHint = "Wrong button!";
    public string TimeUpHint = "Time's up!";

    public UnityEvent OnWrongButton;
    public UnityEvent OnSolved;
    public UnityEvent OnFailed;

    private int _wantedTotal;
    private int _wantedInJar;
    private int _nextSortingOrder;
    private Vector3 _pickUpPosition;

    void Start()
    {
        Dragger.OnPickUp.AddListener(HandlePickUp);
        Dragger.OnRelease.AddListener(HandleRelease);
        Dragger.OnSlip.AddListener(HandleSlip);
        Timer.OnTimeUp.AddListener(HandleTimeUp);

        HintLabel.text = "";
        Begin();
    }

    private void Begin()
    {
        _wantedTotal = 0;
        _wantedInJar = 0;
        _nextSortingOrder = 0;

        SpillButtons();
        UpdateProgress();
        Dragger.enabled = true;
        Timer.Run();
    }

    private void Restart()
    {
        foreach (var button in GetComponentsInChildren<SortingButton>(true))
        {
            Destroy(button.gameObject);
        }

        Begin();
    }

    private void SpillButtons()
    {
        // Shuffled so the kinds end up mixed in the pile instead of lying in layers
        var kinds = ButtonKinds
            .SelectMany(v => Enumerable.Repeat(v, v.Count))
            .OrderBy(_ => Random.value);
        var takenSpots = new List<Vector3>();

        foreach (var kind in kinds)
        {
            var spot = FindSpot(takenSpots);
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

    private Vector3 FindSpot(List<Vector3> takenSpots)
    {
        var area = Table.bounds;

        if (MessyPile)
        {
            // A heap around the middle of the table, the buttons are free to cover each other
            var offset = new Vector3(Random.Range(-0.5f, 0.5f) * area.size.x, Random.Range(-0.5f, 0.5f) * area.size.y);
            return area.center + offset * PileSize;
        }

        // Tries a few random spots and keeps the one farthest from the other buttons, which
        // spreads them out evenly without needing a grid
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

        if (!Jar.Contains(position))
        {
            // Buttons can only lie on the table, one dropped anywhere else slides back onto it
            if (!Table.OverlapPoint(position)) button.SlideTo(Table.ClosestPoint(position), SlideDuration);
            return;
        }

        if (!IsWanted(button))
        {
            // The jar spits the button back to where it was taken from
            button.SlideTo(_pickUpPosition, SlideDuration);
            Timer.Penalize(WrongButtonPenalty);
            ShowHint($"{WrongButtonHint}  -{WrongButtonPenalty:0} s");
            OnWrongButton.Invoke();
            return;
        }

        button.DropInto(Jar);
        _wantedInJar++;
        UpdateProgress();

        if (_wantedInJar < _wantedTotal) return;

        Timer.Stop();
        Dragger.enabled = false;
        OnSolved.Invoke();
    }

    private void HandleSlip(SortingButton button, Vector2 velocity)
    {
        // The button flies on in the direction it was carried, but always lands on the table
        var landing = (Vector2)button.transform.position + velocity * SlipMomentum;
        button.SlideTo(Table.ClosestPoint(landing), SlideDuration);
        ShowHint(SlipHint);
    }

    private void HandleTimeUp()
    {
        Dragger.enabled = false;
        ShowHint(TimeUpHint);
        OnFailed.Invoke();
        Invoke(nameof(Restart), RestartDelay);
    }

    private void ShowHint(string text)
    {
        HintLabel.text = text;
        // Shown at once, then it fades away on its own
        HintLabel.canvasRenderer.SetAlpha(1.0f);
        HintLabel.CrossFadeAlpha(0.0f, HintDuration, false);
    }

    private void UpdateProgress()
    {
        ProgressLabel.text = $"{_wantedInJar} / {_wantedTotal}";
    }
}
