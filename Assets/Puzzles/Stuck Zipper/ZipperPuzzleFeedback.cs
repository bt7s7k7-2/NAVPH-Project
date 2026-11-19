using UnityEngine;

public class ZipperPuzzleFeedback : MonoBehaviour
{
    public string StatusText = "Draw the pattern on the teddy's belly";
    public bool IsSolved { get; private set; }

    public void SetStatus(string text)
    {
        if (!IsSolved)
        {
            StatusText = text;
        }
    }

    public void MarkSolved()
    {
        IsSolved = true;
        StatusText = "Pattern matched! Zipper fixed!";
        Debug.Log(StatusText);
    }

    public void MarkFailed()
    {
        if (!IsSolved)
        {
            StatusText = "Not quite — try drawing the zigzag again!";
        }
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Box(new Rect(20, 20, 460, 44), StatusText, style);
    }
}
