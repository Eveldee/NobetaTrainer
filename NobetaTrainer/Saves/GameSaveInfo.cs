using System;
using Humanizer;
using MarsSDK;

namespace NobetaTrainer.Saves;

public record GameSaveInfo(int Index, string StageName, GameDifficulty Difficulty, int ClearedCount)
{
    public required string LastSaveTimestamp { get; init; }

    public static GameSaveInfo FromSaveIndex(int index)
    {
        if (Game.ReadGameSave(index, out var gameSave) == ReadFileResult.Succeed)
        {
            var preview = new GameSavePreviewData();
            preview.Apply(gameSave);

            return new GameSaveInfo(index, Game.GetLocationText(preview.stage, preview.savePoint), preview.difficulty, preview.gameCleared)
            {
                 LastSaveTimestamp = DateTime.Parse(preview.timeStamp).ToLocalTime().Humanize()
            };
        }

        return null;
    }
}