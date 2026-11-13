using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DialogueMachine;
using TMPro;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles all NPC interactions via dialogue
/// </summary>
public partial class DialogueGui : MonoBehaviour, IDialogueHost
{
    public TextMeshProUGUI CharacterNameElement;
    public TextMeshProUGUI DialogueTextElement;
    public Button ContinueButton;
    public Button ChoiceOptionButtonTemplate;
    public float TextRevealSpeed = 10.0f;
    public Image LeftSprite;
    public Image RightSprite;

    [Serializable]
    public class CharacterInfo
    {
        public enum DisplayPositionType
        {
            Left, Right,
        }

        public string Name;
        public Sprite DefaultSprite;
        public DisplayPositionType DisplayPosition;
        [SerializeField] public Dictionary<string, Sprite> Sprites;
    }
    [SerializeField] public Dictionary<string, CharacterInfo> Characters;

    private DialogueManager<DialogueImpl> _dialogue;
    private string _textToReveal = "";
    private float _revealIndex = 0;

    [AutoStaticsCleanup] public static DialogueGui Instance { get; private set; } = null;

    private enum State
    {
        Inactive,
        Uninitialised,
        RevealingText,
        WaitingForUser
    }

    private State _state = State.Inactive;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _dialogue = new DialogueManager<DialogueImpl>(DialogueState.GetInstance().Machine, this);
        ContinueButton.gameObject.SetActive(false);
        ChoiceOptionButtonTemplate.gameObject.SetActive(false);
        CharacterNameElement.text = "";
        DialogueTextElement.text = "";

        ContinueButton.onClick.AddListener(Continue);
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Continue()
    {
        if (_state == State.RevealingText)
        {
            // If we are currently revealing text, the continue button skips the animation
            _revealIndex = _textToReveal.Length;
        }
        else
        {
            // Otherwise go to the next line of dialogue
            if (_dialogue.IsAborted) SetState(State.Inactive);
            else _dialogue.Update(null);
        }
    }

    private static readonly Regex _spriteNameInDialogue = new Regex(@"^(.*)#(\w+)$");

    void IDialogueHost.EmitDialogue(string subject, string text)
    {
        Debug.Log($"{subject} \\ {text}");

        if (_spriteNameInDialogue.Match(text) is { Success: true } match)
        {
            text = match.Groups[1].Value;
            var spriteName = match.Groups[2].Value;
            ShowSprite(subject[2..], spriteName);
        }

        if (subject == "c_player")
        {
            // Do not display the lines said by the player character, the player has already read them when choosing an options
            // TODO: Handle cases where there are player character lines not chosen by the player
            return;
        }

        CharacterNameElement.text = Characters[subject[2..]].Name;
        _textToReveal = text.Trim();
        SetState(State.RevealingText);
    }

    protected void ShowSprite(string character, string spriteName)
    {
        Debug.Log($"Dialogue sprite: {character}::{spriteName ?? "<null>"}");
        var info = Characters[character];
        try
        {
            var sprite = spriteName == null ? info.DefaultSprite : info.Sprites[spriteName];

            (info.DisplayPosition == CharacterInfo.DisplayPositionType.Left ? LeftSprite : RightSprite).sprite = sprite;
        }
        catch (KeyNotFoundException)
        {
            Debug.LogWarning($"Invalid sprite name {character}::{spriteName}");
        }
    }

    private static readonly Regex _characterNameInLabel = new Regex(@"^npc_(\w+)");

    public void StartDialogue(string label)
    {
        if (_characterNameInLabel.Match(label) is { Success: true } match)
        {
            ShowSprite(match.Groups[1].Value, null);
        }

        ShowSprite("player", null);

        SetState(State.Uninitialised);
        _dialogue.Jump(label);

    }

    void SetState(State newState)
    {
        if (_state == newState) return;
        _state = newState;

        switch (_state)
        {
            case State.Inactive:
                gameObject.SetActive(false);
                // The dialogue has ended, resume player character control
                InputSystem.actions.FindActionMap("Player", throwIfNotFound: true).Enable();
                break;

            case State.Uninitialised:
                gameObject.SetActive(true);
                SetState(State.RevealingText);
                // Disable player character control so they can't move while the talking
                InputSystem.actions.FindActionMap("Player", throwIfNotFound: true).Disable();
                break;

            case State.RevealingText:
                ContinueButton.gameObject.SetActive(value: true);
                ContinueButton.Select();
                ChoiceOptionButtonTemplate.transform.parent.gameObject.SetActive(false);
                _revealIndex = 0;
                DialogueTextElement.text = "";
                break;

            case State.WaitingForUser:
                if (_dialogue.Options is { } options)
                {
                    // Display multiple options if specified
                    ContinueButton.gameObject.SetActive(value: false);
                    var optionsContainer = ChoiceOptionButtonTemplate.transform.parent;
                    optionsContainer.gameObject.SetActive(true);

                    // Clear stale option buttons
                    foreach (var child in Enumerable.Range(0, optionsContainer.childCount)
                        .Select(optionsContainer.GetChild)
                        .Where(v => v != ChoiceOptionButtonTemplate.transform).ToList())
                    {
                        Destroy(child.gameObject);
                    }

                    // Create a button for each option
                    var isFirst = true;
                    foreach (var option in options)
                    {
                        var button = Instantiate(ChoiceOptionButtonTemplate);
                        button.transform.SetParent(optionsContainer, worldPositionStays: false);
                        button.gameObject.SetActive(true);

                        var text = option.Text;
                        if (_spriteNameInDialogue.Match(text) is { Success: true } match)
                        {
                            text = match.Groups[1].Value;
                        }

                        button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;

                        button.onClick.AddListener(() =>
                        {
                            _dialogue.Update(option);
                        });

                        if (isFirst)
                        {
                            isFirst = false;
                            button.Select();
                        }
                    }
                }
                else
                {
                    ContinueButton.Select();
                }
                break;
        }

    }

    void Update()
    {
        if (_state != State.RevealingText) return;
        // Animate revealing text
        var revealBorderIndex = Mathf.FloorToInt(_revealIndex);
        DialogueTextElement.text = revealBorderIndex >= _textToReveal.Length ? _textToReveal : _textToReveal[..revealBorderIndex] + "<alpha=#00>" + _textToReveal[revealBorderIndex..];

        if (_revealIndex >= _textToReveal.Length)
        {
            SetState(State.WaitingForUser);
            return;
        }

        _revealIndex += Time.unscaledDeltaTime * TextRevealSpeed;
        ContinueButton.gameObject.SetActive(true);
    }
}
