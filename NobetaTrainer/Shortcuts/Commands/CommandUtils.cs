﻿using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using NobetaTrainer.Colliders;
using NobetaTrainer.Overlay;
using NobetaTrainer.Teleportation;
using NobetaTrainer.Timer;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;
using static NobetaTrainer.Shortcuts.Commands.CommandType;

namespace NobetaTrainer.Shortcuts.Commands;

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
                Toggle(ref AppearancePatches.ForceNobetaMoveset);
                AppearancePatches.ToggleNobetaMoveset();
            })
        },
        { GiveHPItem, new ShortcutEditor.TrainerCommand(GiveHPItem, ItemPatches.GiveHPItem) },
        { GiveMPItem, new ShortcutEditor.TrainerCommand(GiveMPItem, ItemPatches.GiveMPItem) },
        { GiveBuffItem, new ShortcutEditor.TrainerCommand(GiveBuffItem, ItemPatches.GiveBuffItem) },
        { SpawnOtherItem, new ShortcutEditor.TrainerCommand(SpawnOtherItem, ItemPatches.SpawnOther) },
        { ToggleNoClip, new ShortcutEditor.TrainerCommand(ToggleNoClip, () => Toggle(ref MovementPatches.NoClipEnabled)) },
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
        { ResetTimers, new ShortcutEditor.TrainerCommand(ResetTimers, () => Singletons.Timers.ResetTimers()) },
        { ToggleColliders, new ShortcutEditor.TrainerCommand(ToggleColliders,
            () =>
            {
                Toggle(ref CollidersRenderPatches.ShowColliders);
                CollidersRenderPatches.ToggleShowColliders();
            })
        },
        { ReloadSaveState, new ShortcutEditor.TrainerCommand(ReloadSaveState, () => Singletons.SavesManager?.ReloadSaveState()) },
        { ToggleVanillaMode, new ShortcutEditor.TrainerCommand(ToggleVanillaMode, () => Toggle(ref CharacterPatches.VanillaMode)) },
        { ToggleInvisible, new ShortcutEditor.TrainerCommand(ToggleInvisible, () =>
            {
                Toggle(ref AppearancePatches.InvisibleEnabled);
                AppearancePatches.UpdateAppearance();
            })
        },
        { HideHud, new ShortcutEditor.TrainerCommand(HideHud, () =>
            {
                Toggle(ref OtherPatches.HideHud);
                UiHelpers.ToggleHudVisibility(!OtherPatches.HideHud);
            })
        }
    };
    public static string[] TrainerCommandNames { get; } = Enum.GetValues<CommandType>().Skip(1).Select(type => type.Humanize(LetterCasing.Title)).ToArray();

    public static void Toggle(ref bool toggleValue)
    {
        toggleValue = !toggleValue;
    }
}