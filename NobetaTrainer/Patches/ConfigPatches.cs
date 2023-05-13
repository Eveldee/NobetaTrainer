using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using HarmonyLib;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Commands;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Patches;

public static class ConfigPatches
{
    public class SavableCommandAction
    {
        public required CommandType CommandType { get; init; }
        public required string HumanReadablePath { get; init; }
        public required string ControlPath { get; init; }
        public required Guid ActionId { get; init; }

        public required bool NeedCtrlModifier { get; init; }
        public required bool NeedAltModifier { get; init; }
        public required bool NeedShiftModifier { get; init; }

        [JsonConstructor]
        public SavableCommandAction()
        {

        }

        [SetsRequiredMembers]
        public SavableCommandAction(ShortcutEditor.CommandAction commandAction)
        {
            CommandType = commandAction.TrainerCommand.CommandType;
            ControlPath = commandAction.ControlPath;
            HumanReadablePath = commandAction.HumanReadablePath;
            ActionId = commandAction.ActionId.ToManaged();

            NeedCtrlModifier = commandAction.NeedCtrlModifier;
            NeedAltModifier = commandAction.NeedAltModifier;
            NeedShiftModifier = commandAction.NeedShiftModifier;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave), new Type[]{ })]
    [HarmonyPostfix]
    public static void WriteGameSavePostfix()
    {
        Plugin.SaveConfigs();
    }
}