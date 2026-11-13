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

    public UnityEvent OnTimeUp;

    public float Remaining { get; private set; }
    public bool IsRunning { get; private set; }

    private int _shownSeconds = -1;

    void Awake()
    {
        Remaining = Duration;
        UpdateLabel();
    }

    void Update()
    {
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

    private void UpdateLabel()
    {
        var seconds = Mathf.CeilToInt(Remaining);
        if (seconds == _shownSeconds) return;

        _shownSeconds = seconds;
        Label.text = $"{seconds / 60}:{seconds % 60:00}";
    }
}
