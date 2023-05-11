using System;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.Collections.Generic;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NobetaTrainer.Behaviours;

// ReSharper disable once ClassNeverInstantiated.Global
public class ShortcutEditor : MonoBehaviour
{
    static ShortcutEditor()
    {
        ClassInjector.RegisterTypeInIl2Cpp<TrainerCommand>();
        ClassInjector.RegisterTypeInIl2Cpp<CommandAction>();
    }

    public class TrainerCommand : Il2CppSystem.Object
    {
        public string Name { get; }
        public Il2CppSystem.Action Execute { get; }

        public TrainerCommand()
        {

        }

        public TrainerCommand(IntPtr ptr) : base(ptr)
        {

        }

        public TrainerCommand(string name, Il2CppSystem.Action execute) : base(ClassInjector.DerivedConstructorPointer<TrainerCommand>())
        {
            ClassInjector.DerivedConstructorBody(this);

            Name = name;
            Execute = execute;
        }

        [HideFromIl2Cpp]
        public TrainerCommand(string name, Action execute) : base(ClassInjector.DerivedConstructorPointer<TrainerCommand>())
        {
            ClassInjector.DerivedConstructorBody(this);

            Name = name;
            Execute = execute;
        }

        public void Deconstruct(out string name, out Il2CppSystem.Action execute)
        {
            name = Name;
            execute = Execute;
        }
    }

    public class CommandAction : Il2CppSystem.Object
    {
        public TrainerCommand TrainerCommand { get; }
        public InputAction InputAction { get; }
        public string ControlPath { get; set; }
        public Il2CppSystem.Guid ActionId { get; set; }

        public required bool NeedCtrlModifier;
        public required bool NeedAltModifier;
        public required bool NeedShiftModifier;

        public CommandAction()
        {

        }

        public CommandAction(IntPtr ptr) : base(ptr)
        {

        }

        public CommandAction(TrainerCommand trainerCommand, InputAction inputAction) : base(ClassInjector.DerivedConstructorPointer<CommandAction>())
        {
            ClassInjector.DerivedConstructorBody(this);

            TrainerCommand = trainerCommand;
            InputAction = inputAction;
        }

        public void UpdateDisplay()
        {
            ControlPath = InputAction.bindings.Count > 0
                ? InputControlPath.ToHumanReadableString(InputAction.bindings[0].effectivePath)
                : "<None> [None]";
            ActionId = InputAction.id;
        }
    }

    public InputAction BuildingInputAction { get; private set; }
    public CommandAction BuildingCommandAction { get; private set; }
    public Dictionary<InputAction, CommandAction> CommandActionsMap { get; } = new();
    public int SelectedCommandIndex;
    public bool Initialized;

    private readonly InputActionMap _inputActionMap = new("NobetaTrainer");
    private InputAction _ctrlModifierAction;
    private InputAction _altModifierAction;
    private InputAction _shiftModifierAction;

    public ShortcutEditor()
    {

    }

    public CommandAction AddCommandAction(TrainerCommand trainerCommand, string controlPath)
    {
        var inputAction = _inputActionMap.AddAction($"{trainerCommand.Name}({Guid.NewGuid()})", InputActionType.Button);
        inputAction.AddBinding(controlPath);

        var commandAction = new CommandAction(trainerCommand, inputAction)
        {
            NeedCtrlModifier = BuildingCommandAction.NeedCtrlModifier,
            NeedAltModifier = BuildingCommandAction.NeedAltModifier,
            NeedShiftModifier = BuildingCommandAction.NeedShiftModifier,
            ActionId = inputAction.id,
            ControlPath = InputControlPath.ToHumanReadableString(controlPath)
        };
        CommandActionsMap[inputAction] = commandAction;

        return commandAction;
    }

    public void CreateShortcut()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            var trainerCommand = CommandUtils.TrainerCommands[SelectedCommandIndex];

            AddCommandAction(trainerCommand, BuildingInputAction.bindings[0].effectivePath);
        });
    }

    private void Awake()
    {

    }

    private void Init()
    {
        if (Initialized)
        {
            return;
        }

        // Wait for unity input management to start
        BuildingInputAction = _inputActionMap.AddAction("ShortcutEditorBuilding", InputActionType.Button);
        BuildingInputAction.AddBinding("<None>/None");
        BuildingCommandAction = new CommandAction(new("", () => { }), BuildingInputAction)
        {
            NeedCtrlModifier = true,
            NeedAltModifier = false,
            NeedShiftModifier = false,
            ActionId = BuildingInputAction.id,
            ControlPath = InputControlPath.ToHumanReadableString(BuildingInputAction.bindings[0].effectivePath)
        };

        // Modifier Actions
        _ctrlModifierAction = _inputActionMap.AddAction("CtrlModifier", InputActionType.Button);
        _ctrlModifierAction.AddBinding("<Keyboard>/ctrl");
        _altModifierAction = _inputActionMap.AddAction("AltModifier", InputActionType.Button);
        _altModifierAction.AddBinding("<Keyboard>/alt");
        _shiftModifierAction = _inputActionMap.AddAction("ShiftModifier", InputActionType.Button);
        _shiftModifierAction.AddBinding("<Keyboard>/shift");

        // Command actions
        var firstCommand = AddCommandAction(CommandUtils.TrainerCommands.First(), "<Keyboard>/d");
        firstCommand.NeedCtrlModifier = true;
        firstCommand.UpdateDisplay();

        // Enable all actions and bindings
        _inputActionMap.Enable();

        Initialized = true;
    }

    private void Update()
    {
        // Disable action map if Shortcut Editor window is visible
        if (_inputActionMap.enabled && Plugin.TrainerOverlay._showShortcutEditorWindow)
        {
            _inputActionMap.Disable();

            return;
        }

        if (!_inputActionMap.enabled && !Plugin.TrainerOverlay._showShortcutEditorWindow)
        {
            _inputActionMap.Enable();
        }

        // Check all actions
        foreach (var commandAction in CommandActionsMap.Values)
        {
            // Check if input is triggered (only true on 1 trigger frame)
            if (commandAction.InputAction.triggered)
            {
                // If a modifier is enabled, it implies that the modifier state must be on (Need => State)
                // As known, the boolean expression for (A => B) is (!A | B)
                // We do this for all the modifiers
                var modifiersValid = (!commandAction.NeedCtrlModifier || _ctrlModifierAction.inProgress)
                                        && (!commandAction.NeedAltModifier || _altModifierAction.inProgress)
                                        && (!commandAction.NeedShiftModifier || _shiftModifierAction.inProgress);

                if (modifiersValid)
                {
                    commandAction.TrainerCommand.Execute.Invoke();
                }
            }
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.SwitchTitleScene))]
    [HarmonyPostfix]
    private static void SwitchTitleScenePostfix()
    {
        Singletons.ShortcutEditor.Init();
    }
}