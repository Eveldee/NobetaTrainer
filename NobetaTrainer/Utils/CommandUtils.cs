using System.Linq;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Patches;

namespace NobetaTrainer.Utils;

public static class CommandUtils
{
    public static readonly ShortcutEditor.TrainerCommand[] TrainerCommands =
    {
        new("Toggle No Damage", () => Toggle(ref CharacterPatches.NoDamageEnabled)),
        new("Toggle Infinite HP", () => Toggle(ref CharacterPatches.InfiniteHpEnabled)),
        new("Toggle Infinite Mana", () => Toggle(ref CharacterPatches.InfiniteManaEnabled)),
        new("Toggle Infinite Stamina", () => Toggle(ref CharacterPatches.InfiniteStaminaEnabled)),
    };
    public static string[] TrainerCommandNames { get; } = TrainerCommands.Select(command => command.Name).ToArray();

    private static void Toggle(ref bool toggleValue)
    {
        toggleValue = !toggleValue;
    }
}