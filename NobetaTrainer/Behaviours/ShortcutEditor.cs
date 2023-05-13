﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppSystem.IO;
using NobetaTrainer.Commands;
using NobetaTrainer.Patches;
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

    public string ShortcutsSavePath { get; } = Path.Combine(Plugin.ConfigDirectory.FullName, "Shortcuts.json");
    public string CommandsSavePath { get; } = Path.Combine(Plugin.ConfigDirectory.FullName, "Commands.json");

    public Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Guid, CommandAction> CommandActionsMap { get; private set; }
    public CommandAction BuildingCommandAction { get; private set; }
    public int SelectedCommandIndex;
    public bool Initialized;

    private InputActionMap _inputActionMap;
    public InputAction BuildingInputAction { get; private set; }
    public const string BuildingInputActionName = "ShortcutEditorBuilding";

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
            ControlPath = InputControlPath.ToHumanReadableString(controlPath)
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
        File.WriteAllText(ShortcutsSavePath, _inputActionMap.ToJson());

        // Create managed dictionary
        var commandActions = new List<ConfigPatches.SavableCommandAction>();
        foreach (var keyValuePair in CommandActionsMap)
        {
            commandActions.Add(new ConfigPatches.SavableCommandAction(keyValuePair.Value));
        }

        File.WriteAllText(CommandsSavePath,  SerializeUtils.SerializeIndented(commandActions));
    }

    private void Awake()
    {

    }

    private void Init()
    {
        void NewMap()
        {
            _inputActionMap = new InputActionMap("NobetaTrainer");

            // Build action
            BuildingInputAction = _inputActionMap.AddAction(BuildingInputActionName, InputActionType.Button);
            BuildingInputAction.AddBinding("<None>/None");

            // Modifier Actions
            _ctrlModifierAction = _inputActionMap.AddAction(CtrlModifierActionName, InputActionType.Button);
            _ctrlModifierAction.AddBinding("<Keyboard>/ctrl");
            _altModifierAction = _inputActionMap.AddAction(AltModifierActionName, InputActionType.Button);
            _altModifierAction.AddBinding("<Keyboard>/alt");
            _shiftModifierAction = _inputActionMap.AddAction(ShiftModifierActionName, InputActionType.Button);
            _shiftModifierAction.AddBinding("<Keyboard>/shift");

            CommandActionsMap = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Guid, CommandAction>();
        }

        if (Initialized)
        {
            return;
        }

        // Check if there is saved shortcuts
        if (File.Exists(ShortcutsSavePath) && File.Exists(CommandsSavePath))
        {
            try
            {
                // Try load shortcuts from them
                var actionMapJson = File.ReadAllText(ShortcutsSavePath);
                var commandsJson = File.ReadAllText(CommandsSavePath);

                var savedMap = InputActionMap.FromJson(actionMapJson)[0];

                _inputActionMap = savedMap;

                BuildingInputAction = _inputActionMap[BuildingInputActionName];
                _ctrlModifierAction = _inputActionMap[CtrlModifierActionName];
                _shiftModifierAction = _inputActionMap[ShiftModifierActionName];
                _altModifierAction = _inputActionMap[AltModifierActionName];

                // Also load commands
                var commands = JsonSerializer.Deserialize<List<ConfigPatches.SavableCommandAction>>(commandsJson);

                CommandActionsMap = new Il2CppSystem.Collections.Generic.Dictionary<Il2CppSystem.Guid, CommandAction>();
                foreach (var commandAction in commands)
                {
                    CommandActionsMap[commandAction.ActionId.ToUnmanaged()] =
                        new CommandAction(CommandUtils.TrainerCommands[commandAction.CommandType], _inputActionMap[commandAction.ActionId.ToString()])
                        {
                            NeedCtrlModifier = commandAction.NeedCtrlModifier,
                            NeedAltModifier = commandAction.NeedAltModifier,
                            NeedShiftModifier = commandAction.NeedShiftModifier,
                            ActionId = commandAction.ActionId.ToUnmanaged(),
                            ControlPath = commandAction.ControlPath
                        };
                }

                Plugin.Log.LogMessage("Loaded shortcuts and commands from saved config");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Couldn't load saved shortcuts, using new shortcut map.");
                Plugin.Log.LogError(e.Message);
                Plugin.Log.LogError(e.StackTrace);

                NewMap();
            }
        }
        else
        {
            Plugin.Log.LogInfo("No shortcuts found, using new shortcut map.");
            NewMap();
        }

        BuildingCommandAction = new CommandAction(new(CommandType.None, () => { }), BuildingInputAction)
        {
            NeedCtrlModifier = true,
            NeedAltModifier = false,
            NeedShiftModifier = false,
            ActionId = BuildingInputAction.id,
            ControlPath = InputControlPath.ToHumanReadableString(BuildingInputAction.bindings[0].effectivePath)
        };

        // Enable all actions and bindings
        _inputActionMap.Enable();

        Initialized = true;

        Plugin.Log.LogMessage("Shortcut Editor initialized");
    }

    private void Update()
    {
        // Disable action map if Shortcut Editor window is visible
        if (_inputActionMap?.enabled == true && Plugin.TrainerOverlay._showShortcutEditorWindow)
        {
            _inputActionMap.Disable();

            return;
        }

        if (_inputActionMap?.enabled == false && !Plugin.TrainerOverlay._showShortcutEditorWindow)
        {
            _inputActionMap.Enable();
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