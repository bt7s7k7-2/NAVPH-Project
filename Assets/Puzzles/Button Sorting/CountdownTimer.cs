using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Counts down a time limit and shows the remaining time.
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    public float Duration = 60.0f;
    public TMP_Text Label;
    public Color PenaltyColor = new Color(1.0f, 0.3f, 0.35f);
    public float PenaltyFlashDuration = 0.8f;

    public UnityEvent OnTimeUp;

    public float Remaining { get; private set; }
    public bool IsRunning { get; private set; }

    private int _shownSeconds = -1;
    private Color _normalColor;
    private float _flash;

    void Awake()
    {
        _normalColor = Label.color;
        Remaining = Duration;
        UpdateLabel();
    }

    void Update()
    {
        if (_flash > 0.0f)
        {
            _flash = Mathf.Max(0.0f, _flash - Time.deltaTime / PenaltyFlashDuration);
            Label.color = Color.Lerp(_normalColor, PenaltyColor, _flash);
        }

        if (!IsRunning) return;

        Remaining = Mathf.Max(0.0f, Remaining - Time.deltaTime);
        UpdateLabel();

        if (Remaining > 0.0f) return;

        IsRunning = false;
        OnTimeUp.Invoke();
    }

    public void Run()
    {
        Remaining = Duration;
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    /// <summary>
    /// Takes time away from the player, the label flashes so the loss does not go unnoticed.
    /// </summary>
    public void Penalize(float seconds)
    {
        Remaining = Mathf.Max(0.0f, Remaining - seconds);
        _flash = 1.0f;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        var seconds = Mathf.CeilToInt(Remaining);
        if (seconds == _shownSeconds) return;

        _shownSeconds = seconds;
        Label.text = $"{seconds / 60}:{seconds % 60:00}";
    }
}
