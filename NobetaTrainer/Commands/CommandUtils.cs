using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Humanizer;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Overlay;
using NobetaTrainer.Patches;
using static NobetaTrainer.Commands.CommandType;

namespace NobetaTrainer.Commands;

public static class CommandUtils
{
    public static readonly Dictionary<CommandType, ShortcutEditor.TrainerCommand> TrainerCommands = new()
    {
        { ToggleOverlay, new ShortcutEditor.TrainerCommand(ToggleOverlay, () => Toggle(ref OverlayState.ShowOverlay)) },
        { ToggleNoDamage, new ShortcutEditor.TrainerCommand(ToggleNoDamage, () => Toggle(ref CharacterPatches.NoDamageEnabled)) },
        { ToggleInfiniteHP, new ShortcutEditor.TrainerCommand(ToggleInfiniteHP, () => Toggle(ref CharacterPatches.InfiniteHpEnabled)) },
        { ToggleInfiniteMana, new ShortcutEditor.TrainerCommand(ToggleInfiniteMana, () => Toggle(ref CharacterPatches.InfiniteManaEnabled)) },
        { ToggleInfiniteStamina, new ShortcutEditor.TrainerCommand(ToggleInfiniteStamina, () => Toggle(ref CharacterPatches.InfiniteStaminaEnabled)) },
        { ToggleNobetaMoveset, new ShortcutEditor.TrainerCommand(ToggleNobetaMoveset, () =>
            {
                Toggle(ref AppearancePatches.UseNobetaMoveset);
                AppearancePatches.ToggleNobetaSkin();
            })
        },
        { GiveHPItem, new ShortcutEditor.TrainerCommand(GiveHPItem, ItemPatches.GiveHPItem) },
        { GiveMPItem, new ShortcutEditor.TrainerCommand(GiveMPItem, ItemPatches.GiveMPItem) },
        { GiveBuffItem, new ShortcutEditor.TrainerCommand(GiveBuffItem, ItemPatches.GiveBuffItem) },
        { SpawnOtherItem, new ShortcutEditor.TrainerCommand(SpawnOtherItem, ItemPatches.SpawnOther) },
        { ToggleNoClip, new ShortcutEditor.TrainerCommand(ToggleNoClip, () =>
            {
                Toggle(ref MovementPatches.NoClipEnabled);
                MovementPatches.ToggleNoClip();
            })
        },
        { ToggleGlide, new ShortcutEditor.TrainerCommand(ToggleGlide, () => Toggle(ref MovementPatches.GlideEnabled)) },
        { ToggleOneTap, new ShortcutEditor.TrainerCommand(ToggleOneTap, () => Toggle(ref CharacterPatches.OneTapEnabled)) },
        { ToggleBrightMode, new ShortcutEditor.TrainerCommand(ToggleBrightMode, () =>
            {
                Toggle(ref OtherPatches.BrightMode);
                OtherPatches.UpdateBrightMode();
            })
        },
        { TeleportToLastPoint, new ShortcutEditor.TrainerCommand(TeleportToLastPoint, TeleportationPatches.TeleportLastPoint) },
        { ToggleTimers, new ShortcutEditor.TrainerCommand(ToggleTimers, () => Toggle(ref Timers.ShowTimers)) },
        { ResetTimers, new ShortcutEditor.TrainerCommand(ResetTimers, () => Singletons.Timers.ResetTimers()) }
    };
    public static string[] TrainerCommandNames { get; } = Enum.GetValues<CommandType>().Skip(1).Select(type => type.Humanize(LetterCasing.Title)).ToArray();

    public static void Toggle(ref bool toggleValue)
    {
        toggleValue = !toggleValue;
    }
}