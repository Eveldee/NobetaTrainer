using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using MarsSDK;
using NobetaTrainer.Trainer;

namespace NobetaTrainer.Saves;

// TODO Add Teleportation Point at some point =))
public record SaveState(Guid Id, string StageName, GameDifficulty Difficulty, int ClearedCount)
{
    public required string SaveName { get; set; }

    public static bool TryCreateFromCurrentSave(string name, [NotNullWhen(true)] out SaveState saveState)
    {
        saveState = null;

        if (Singletons.GameSave?.basic?.dataIndex is not { } saveIndex)
        {
            return false;
        }

        if (Game.ReadGameSave(saveIndex, out var gameSave) != ReadFileResult.Succeed)
        {
            return false;
        }

        var preview = new GameSavePreviewData();
        preview.Apply(gameSave);

        // Set file index to SaveStateIndex
        gameSave.basic.dataIndex = SavesManager.SaveStateIndex;
        Game.WriteGameSave(gameSave);

        saveState = new SaveState(
            Guid.NewGuid(),
            Game.GetLocationText(preview.stage, preview.savePoint),
            preview.difficulty,
            preview.gameCleared
        ) { SaveName = name };

        var sourcePath = SavesManager.GetGameSavePathFromIndex(SavesManager.SaveStateIndex);
        var destinationPath = SavesManager.GetGameSaveStatePathFromGuid(saveState.Id);

        if (!File.Exists(sourcePath))
        {
            Plugin.Log.LogError(
                $"A save at location {sourcePath} should exist at this point, an error probably occured during save copy"
            );

            return false;
        }

        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        // Copy save at index SaveStateIndex to SaveStates folder
        File.Copy(SavesManager.GetGameSavePathFromIndex(SavesManager.SaveStateIndex), SavesManager.GetGameSaveStatePathFromGuid(saveState.Id));

        return true;
    }
}