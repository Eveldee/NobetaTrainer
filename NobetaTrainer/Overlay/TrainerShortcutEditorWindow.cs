﻿using System;
using System.Linq;
using System.Numerics;
using Humanizer;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ImGuiNET;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Shortcuts;
using NobetaTrainer.Shortcuts.Commands;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;
using UnityEngine.InputSystem;

namespace NobetaTrainer.Overlay;

public partial class NobetaTrainerOverlay
{
    private void ShowShortcutEditorWindow()
    {
        void OnCompleteRebind(InputActionRebindingExtensions.RebindingOperation operation)
        {
            // Update associated command action
            if (!Singletons.ShortcutEditor.CommandActionsMap.TryGetValue(operation.action.id, out var commandAction))
            {
                commandAction = Singletons.ShortcutEditor.BuildingCommandAction;
            }

            commandAction.UpdatePathAndId();

            operation.Dispose();
        }

        void DisplayCommandAction(ShortcutEditor.CommandAction commandAction, bool deleteButton = true, int pathOffset = 0)
        {
            DisplayCommandActionCustom(commandAction, ref commandAction.NeedCtrlModifier, ref commandAction.NeedAltModifier, ref commandAction.NeedShiftModifier, OnCompleteRebind, deleteButton, pathOffset);
        }

        void DisplayCommandActionCustom(ShortcutEditor.CommandAction commandAction, ref bool ctrlModifier, ref bool altModifier, ref bool shiftModifier, Action<InputActionRebindingExtensions.RebindingOperation> customAction, bool deleteButton = true, int pathOffset = 0)
        {
            if (commandAction.TrainerCommand.CommandType != CommandType.None)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextColored(ValueColor, commandAction.TrainerCommand.CommandType.Humanize(LetterCasing.Title));
                ImGui.SameLine(180);
            }

            if (ImGui.Button($"{commandAction.HumanReadablePath}##{commandAction.ActionId}"))
            {
                NobetaProcessUtils.FocusNobetaWindow();

                Singletons.Dispatcher.Enqueue(() =>
                {
                    var rebind = commandAction.InputAction.PerformInteractiveRebinding()
                        .WithCancelingThrough("<Keyboard>/escape");

                    rebind.OnComplete(customAction);

                    rebind.Start();
                });
            }


            ImGui.SameLine(pathOffset == 0 ? 0 : 195 + pathOffset * 7);
            ImGui.Checkbox($"Ctrl##{commandAction.ActionId}", ref ctrlModifier);
            ImGui.SameLine();
            ImGui.Checkbox($"Alt##{commandAction.ActionId}", ref altModifier);
            ImGui.SameLine();
            ImGui.Checkbox($"Shift##{commandAction.ActionId}", ref shiftModifier);

            if (deleteButton)
            {
                ImGui.SameLine();
                if (ButtonColored(ErrorButtonColor, $"Delete##{commandAction.ActionId}"))
                {
                    Singletons.Dispatcher.Enqueue(() =>
                    {
                        Singletons.ShortcutEditor.DeleteCommandAction(commandAction);
                    });
                }
            }
        }

        ImGui.Begin("Shortcut Editor", ref OverlayState.ShowShortcutEditorWindow);
        ImGui.PushTextWrapPos();

        ImGui.TextColored(InfoColor, "Here you can assign shortcuts to NobetaTrainer commands");
        ImGui.TextColored(ErrorColor, "!! Shortcuts are deactivated while this window is shown !!");

        var editor = Singletons.ShortcutEditor;

        if (editor is not { Initialized: true })
        {
            ImGui.Text("Waiting for Shortcut Editor to be loaded...");
        }
        else
        {
            if (ImGui.CollapsingHeader("Active Shortcuts", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Copy collection beforehand to avoid show while modification
                var array = new Il2CppReferenceArray<ShortcutEditor.CommandAction>(editor.CommandActionsMap.Count);
                editor.CommandActionsMap.Values.CopyTo(array, 0);

                Child("Commands", new Vector2(0, 190f), true, ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.HorizontalScrollbar, () =>
                {
                    foreach (var commandAction in array)
                    {
                        DisplayCommandAction(commandAction, true, array.Max(a => a.HumanReadablePath.Length));
                    }
                });
            }

            if (ImGui.CollapsingHeader("Unlock Cursor"))
            {
                DisplayCommandActionCustom(
                    editor.CursorUnlockCommandAction,
                    ref CursorUnlocker.NeedCtrlModifier,
                    ref CursorUnlocker.NeedAltModifier,
                    ref CursorUnlocker.NeedShiftModifier,
                    operation =>
                    {
                        CursorUnlocker.ControlPath = operation.action.bindings[0].effectivePath;
                        editor.CursorUnlockCommandAction.UpdatePathAndId();
                        operation.Dispose();
                    }, false
                );
            }

            if (ImGui.CollapsingHeader("Create Shortcut", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (editor.BuildingCommandAction is not null)
                {
                    ImGui.Combo("Command", ref editor.SelectedCommandIndex, CommandUtils.TrainerCommandNames,
                        CommandUtils.TrainerCommandNames.Length);

                    DisplayCommandAction(editor.BuildingCommandAction, deleteButton: false);

                    if (ButtonColored(PrimaryButtonColor, "Create"))
                    {
                        editor.CreateShortcut();
                    }
                }
            }
        }

        ImGui.PopTextWrapPos();
        ImGui.End();
    }
}