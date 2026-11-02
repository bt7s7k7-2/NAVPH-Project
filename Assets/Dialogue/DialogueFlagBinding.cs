using UnityEngine;

/// <summary>
/// Activates or deactivates an object based on flag state to
/// </summary>
public class DialogueFlagBinding : MonoBehaviour
{
    // TODO: Check multiple flags to handle showing an object only when required while also removing it when picked up
    public string Flag;
    public bool ActiveState;

    private DialogueState.FlagReference _flagState => DialogueState.GetInstance().GetFlag(Flag);

    void Start()
    {
        if (_flagState.Value != ActiveState)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetFlag(bool state)
    {
        _flagState.Value = state;

        if (state != ActiveState)
        {
            gameObject.SetActive(false);
        }
    }
}
