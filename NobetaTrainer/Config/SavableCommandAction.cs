using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Commands;

namespace NobetaTrainer.Config;

public class SavableCommandAction
{
    public required CommandType CommandType { get; init; }
    public required string HumanReadablePath { get; init; }
    public required string ControlPath { get; init; }

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

        NeedCtrlModifier = commandAction.NeedCtrlModifier;
        NeedAltModifier = commandAction.NeedAltModifier;
        NeedShiftModifier = commandAction.NeedShiftModifier;
    }
}