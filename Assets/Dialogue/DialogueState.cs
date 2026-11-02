using System;
using System.Collections.Generic;
using System.Reflection;
using DialogueMachine;
using Unity.Scripting.LifecycleManagement;

/// <summary>
/// Global game state. This class provides an easy access to the current dialogue and, as state is
/// stored and defined in the dialogue definition, also provides wrappers for boolean flags. These
/// can be accessed via string, and therefore configurable in editor.
/// </summary>
public sealed partial class DialogueState
{
    public DialogueImpl Machine { get; private set; }
    public class FlagReference
    {
        protected Func<bool> _getter;
        protected Action<bool> _setter;

        public FlagReference(Func<bool> getter, Action<bool> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public bool Value
        {
            get => _getter();
            set => _setter(value);
        }
    }
    private Dictionary<string, FlagReference> _flags = null;

    public FlagReference GetFlag(string name)
    {
        if (_flags == null)
        {
            _flags = new();

            foreach (var field in typeof(DialogueImpl).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType != typeof(bool)) continue;

                _flags[field.Name] = new FlagReference(
                    () => (bool)field.GetValue(Machine),
                    value => field.SetValue(Machine, value)
                );
            }
        }

        return _flags[name];
    }

    private DialogueState()
    {
        Machine = new DialogueImpl(null);
    }

    [AutoStaticsCleanup] private static DialogueState _instance = null;
    public static DialogueState GetInstance() => _instance ??= new DialogueState();
}
