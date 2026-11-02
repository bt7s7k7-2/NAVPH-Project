using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public string Label;

    public void Trigger()
    {
        Debug.Log($"DialogueTrigger: {Label}");
        var gui = DialogueGui.Instance;
        if (gui == null) return;

        gui.StartDialogue(Label);
    }
}
