using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Humanizer;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Patches;
using static NobetaTrainer.Commands.CommandType;

namespace NobetaTrainer.Commands;

public static class CommandUtils
{
    public static readonly Dictionary<CommandType, ShortcutEditor.TrainerCommand> TrainerCommands = new()
    {
        { ToggleNoDamage, new ShortcutEditor.TrainerCommand(ToggleNoDamage, () => Toggle(ref CharacterPatches.NoDamageEnabled)) },
        { ToggleInfiniteHP, new ShortcutEditor.TrainerCommand(ToggleInfiniteHP, () => Toggle(ref CharacterPatches.InfiniteHpEnabled)) },
        { ToggleInfiniteMana, new ShortcutEditor.TrainerCommand(ToggleInfiniteMana, () => Toggle(ref CharacterPatches.InfiniteManaEnabled)) },
        { ToggleInfiniteStamina, new ShortcutEditor.TrainerCommand(ToggleInfiniteStamina, () => Toggle(ref CharacterPatches.InfiniteStaminaEnabled)) },
    };
    public static string[] TrainerCommandNames { get; } = Enum.GetValues<CommandType>().Skip(1).Select(type => type.Humanize(LetterCasing.Title)).ToArray();

    private static void Toggle(ref bool toggleValue)
    {
        toggleValue = !toggleValue;
    }
}