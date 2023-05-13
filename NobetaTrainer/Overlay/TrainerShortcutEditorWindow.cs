﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using Humanizer;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ImGuiNET;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Commands;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine.InputSystem;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay
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

        void DisplayCommandAction(ShortcutEditor.CommandAction commandAction, bool deleteButton = true)
        {
            if (commandAction.TrainerCommand.CommandType != CommandType.None)
            {
                ImGui.TextColored(ValueColor, commandAction.TrainerCommand.CommandType.Humanize(LetterCasing.Title));
                ImGui.SameLine();
            }

            if (ImGui.Button($"{commandAction.HumanReadablePath}##{commandAction.ActionId}"))
            {
                NobetaProcessUtils.FocusNobetaWindow();

                Singletons.Dispatcher.Enqueue(() =>
                {
                    var rebind = commandAction.InputAction.PerformInteractiveRebinding()
                        .WithCancelingThrough("<Keyboard>/escape");

                    rebind.OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>) OnCompleteRebind);

                    rebind.Start();
                });
            }

            ImGui.SameLine();
            ImGui.Checkbox($"Ctrl##{commandAction.ActionId}", ref commandAction.NeedCtrlModifier);
            ImGui.SameLine();
            ImGui.Checkbox($"Alt##{commandAction.ActionId}", ref commandAction.NeedAltModifier);
            ImGui.SameLine();
            ImGui.Checkbox($"Shift##{commandAction.ActionId}", ref commandAction.NeedShiftModifier);

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

        ImGui.Begin("Shortcut Editor", ref _showShortcutEditorWindow);
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

                foreach (var commandAction in array)
                {
                    DisplayCommandAction(commandAction);
                }
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