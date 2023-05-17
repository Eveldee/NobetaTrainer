using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using NobetaTrainer.Commands;
using NobetaTrainer.Config;
using NobetaTrainer.Overlay;
using NobetaTrainer.Patches;
using NobetaTrainer.Serialization;
using NobetaTrainer.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using File = System.IO.File;

namespace NobetaTrainer.Behaviours;

// ReSharper disable once ClassNeverInstantiated.Global
public class ShortcutEditor : MonoBehaviour
{
    static ShortcutEditor()
    {
        // EnumInjector.RegisterEnumInIl2Cpp<CommandType>();
        // ClassInjector.RegisterTypeInIl2Cpp<TrainerCommand>();
        ClassInjector.RegisterTypeInIl2Cpp<CommandAction>();
    }

    public class TrainerCommand : Il2CppSystem.Object
    {
        public CommandType CommandType { get; }
        public Il2CppSystem.Action Execute { get; }

        public TrainerCommand()
        {

        }

        public TrainerCommand(IntPtr ptr) : base(ptr)
        {

        }

        public TrainerCommand(CommandType commandType, Action execute)
        {
            CommandType = commandType;
            Execute = execute;
        }

        [HideFromIl2Cpp]
        public void Deconstruct(out CommandType commandType, out Il2CppSystem.Action execute)
        {
            commandType = CommandType;
            execute = Execute;
        }
    }

    public class CommandAction : Il2CppSystem.Object
    {
        public TrainerCommand TrainerCommand { get; }
        public InputAction InputAction { get; }
        public string HumanReadablePath { get; set; }
        public required string ControlPath { get; set; }
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

        public void UpdatePathAndId()
        {
            ControlPath = InputAction.bindings.Count > 0
                ? InputAction.bindings[0].effectivePath
                : "<None>/None";
            HumanReadablePath = InputAction.bindings.Count > 0
                ? InputControlPath.ToHumanReadableString(InputAction.bindings[0].effectivePath)
                : "None [None]";
            ActionId = InputAction.id;
        }
    }

    public string CommandsSavePath { get; } = Path.Combine(Plugin.ConfigDirectory.FullName, "ShortcutCommands.json");

    public Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Guid, CommandAction> CommandActionsMap { get; private set; }
    public int SelectedCommandIndex;
    public bool Initialized;

    private InputActionMap _inputActionMap;

    public CommandAction BuildingCommandAction { get; private set; }
    public InputAction BuildingInputAction { get; private set; }
    public const string BuildingInputActionName = "ShortcutEditorBuilding";

    public CommandAction CursorUnlockCommandAction;
    private InputAction _cursorUnlockAction;
    private const string _cursorUnlockActionName = "CursorUnlock";

    private InputAction _ctrlModifierAction;
    private InputAction _altModifierAction;
    private InputAction _shiftModifierAction;
    private const string CtrlModifierActionName = "CtrlModifier";
    private const string AltModifierActionName = "AltModifier";
    private const string ShiftModifierActionName = "ShiftModifier";

    public ShortcutEditor()
    {

    }

    public CommandAction AddCommandAction(TrainerCommand trainerCommand, string controlPath)
    {
        var inputAction = _inputActionMap.AddAction($"{trainerCommand.CommandType}({Guid.NewGuid()})", InputActionType.Button);
        inputAction.AddBinding(controlPath);

        var commandAction = new CommandAction(trainerCommand, inputAction)
        {
            NeedCtrlModifier = BuildingCommandAction.NeedCtrlModifier,
            NeedAltModifier = BuildingCommandAction.NeedAltModifier,
            NeedShiftModifier = BuildingCommandAction.NeedShiftModifier,
            ActionId = inputAction.id,
            HumanReadablePath = InputControlPath.ToHumanReadableString(controlPath),
            ControlPath = controlPath
        };
        CommandActionsMap[inputAction.id] = commandAction;

        return commandAction;
    }

    public void DeleteCommandAction(CommandAction commandAction)
    {
        // Also remove associated input action and bindings
        commandAction.InputAction.RemoveAction();

        CommandActionsMap.Remove(commandAction.ActionId);
    }

    public void CreateShortcut()
    {
        var trainerCommand = CommandUtils.TrainerCommands[(CommandType)(SelectedCommandIndex + 1)];

        Singletons.Dispatcher.Enqueue(() =>
        {
            AddCommandAction(trainerCommand, BuildingInputAction.bindings[0].effectivePath);
        });
    }


    public void SaveShortcuts()
    {
        // Store into managed List that is serializable
        var commandActions = new List<SavableCommandAction>();
        foreach (var keyValuePair in CommandActionsMap)
        {
            commandActions.Add(new SavableCommandAction(keyValuePair.Value));
        }

        File.WriteAllText(CommandsSavePath,  SerializeUtils.SerializeIndented(commandActions));
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

        // Init map
        _inputActionMap = new InputActionMap("NobetaTrainer");

        // Build action
        BuildingInputAction = _inputActionMap.AddAction(BuildingInputActionName, InputActionType.Button);
        BuildingInputAction.AddBinding("<None>/None");

        // Unlock cursor action
        _cursorUnlockAction = _inputActionMap.AddAction(_cursorUnlockActionName, InputActionType.Button);
        _cursorUnlockAction.AddBinding(CursorUnlocker.ControlPath);

        // Modifier Actions
        _ctrlModifierAction = _inputActionMap.AddAction(CtrlModifierActionName, InputActionType.Button);
        _ctrlModifierAction.AddBinding("<Keyboard>/ctrl");
        _altModifierAction = _inputActionMap.AddAction(AltModifierActionName, InputActionType.Button);
        _altModifierAction.AddBinding("<Keyboard>/alt");
        _shiftModifierAction = _inputActionMap.AddAction(ShiftModifierActionName, InputActionType.Button);
        _shiftModifierAction.AddBinding("<Keyboard>/shift");

        CommandActionsMap = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Guid, CommandAction>();

        // Check if there is saved shortcuts
        if (File.Exists(CommandsSavePath))
        {
            try
            {
                // Try load shortcuts from them
                var commandsJson = File.ReadAllText(CommandsSavePath);
                var commands = JsonSerializer.Deserialize<List<SavableCommandAction>>(commandsJson);

                // For each saved command, add it to commands list and recreate InputAction and InputBinding
                foreach (var commandAction in commands)
                {
                    // Recreate InputAction and InputBinding
                    var inputAction = _inputActionMap.AddAction($"{commandAction.CommandType}({Guid.NewGuid()})", InputActionType.Button);
                    inputAction.AddBinding(commandAction.ControlPath);

                    // Add command
                    CommandActionsMap[inputAction.id] =
                        new CommandAction(CommandUtils.TrainerCommands[commandAction.CommandType], inputAction)
                        {
                            NeedCtrlModifier = commandAction.NeedCtrlModifier,
                            NeedAltModifier = commandAction.NeedAltModifier,
                            NeedShiftModifier = commandAction.NeedShiftModifier,
                            ActionId = inputAction.id,
                            HumanReadablePath = commandAction.HumanReadablePath,
                            ControlPath = commandAction.ControlPath
                        };
                }

                Plugin.Log.LogMessage("Loaded shortcuts and commands from saved config");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Couldn't load saved shortcuts, using empty shortcut map.");
                Plugin.Log.LogError(e.Message);
                Plugin.Log.LogError(e.StackTrace);
            }
        }
        else
        {
            Plugin.Log.LogInfo("No shortcuts found, using empty shortcut map.");
        }

        // CommandActions
        BuildingCommandAction = new CommandAction(new(CommandType.None, () => { }), BuildingInputAction)
        {
            NeedCtrlModifier = true,
            NeedAltModifier = false,
            NeedShiftModifier = false,
            ActionId = BuildingInputAction.id,
            HumanReadablePath = InputControlPath.ToHumanReadableString(BuildingInputAction.bindings[0].effectivePath),
            ControlPath = BuildingInputAction.bindings[0].effectivePath
        };

        CursorUnlockCommandAction = new CommandAction(new(CommandType.UnlockCursor, () => CommandUtils.Toggle(ref CursorUnlocker.IsCursorUnlocked)), _cursorUnlockAction)
        {
            NeedCtrlModifier = false,
            NeedAltModifier = false,
            NeedShiftModifier = false,
            ActionId = _cursorUnlockAction.id,
            HumanReadablePath = InputControlPath.ToHumanReadableString(_cursorUnlockAction.bindings[0].effectivePath),
            ControlPath = _cursorUnlockAction.bindings[0].effectivePath
        };

        // Enable all actions and bindings
        _inputActionMap.Enable();

        Initialized = true;

        Plugin.Log.LogMessage("Shortcut Editor initialized");
    }

    private void Update()
    {
        // Disable action map if Shortcut Editor window is visible
        if (_inputActionMap?.enabled == true && OverlayState.ShowShortcutEditorWindow)
        {
            _inputActionMap.Disable();

            return;
        }

        if (_inputActionMap?.enabled == false && !OverlayState.ShowShortcutEditorWindow)
        {
            _inputActionMap.Enable();
        }

        // Unlock cursor
        if (_cursorUnlockAction?.triggered == true)
        {
            var modifiersValid = (!CursorUnlocker.NeedCtrlModifier || _ctrlModifierAction.inProgress)
                                 && (!CursorUnlocker.NeedAltModifier || _altModifierAction.inProgress)
                                 && (!CursorUnlocker.NeedShiftModifier || _shiftModifierAction.inProgress);

            if (modifiersValid)
            {
                CursorUnlockCommandAction.TrainerCommand.Execute.Invoke();
            }
        }

        // Check all actions
        if (CommandActionsMap is null)
        {
            return;
        }

        foreach (var commandAction in CommandActionsMap.Values)
        {
            // Check if input is triggered (only true on 1 trigger frame)
            if (commandAction.InputAction.triggered)
            {
                // If a modifier is enabled, it implies that the modifier state must be on (Need => State)
                // As known, the boolean expression for (A => B) is (!A | B)
                // We do this for each modifiers
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