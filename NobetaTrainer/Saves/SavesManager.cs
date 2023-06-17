using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Il2CppSystem.IO;
using MarsSDK;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace NobetaTrainer.Saves;

public class SavesManager
{
    public static readonly DirectoryInfo NobetaSavesDirectory = new(
        Path.Combine(Plugin.ConfigDirectory.FullName, "..", "..", "..", "LittleWitchNobeta_Data", "Save")
    );

    public IEnumerable<GameSaveInfo> GameSaveInfos => _gameSaveInfos;
    public IEnumerable<SaveState> SaveStates => _saveStates;

    public bool IsLoading { get; set; } = true;

    private readonly GameSaveInfo[] _gameSaveInfos;
    private readonly List<SaveState> _saveStates;

    public SavesManager()
    {
        _gameSaveInfos = Enumerable.Range(1, 9)
            .Where(index => File.Exists(GetGameSavePathFromIndex(index)))
            .Select(GameSaveInfo.FromSaveIndex)
            .ToArray();

        // TODO Load from json file
        _saveStates = new List<SaveState>();
    }

    public void LoadGameSave(GameSaveInfo gameSaveInfo)
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            if (SceneUtils.IsLoading())
            {
                return;
            }

            if (Game.ReadGameSave(gameSaveInfo.Index, out var gameSave) == ReadFileResult.Succeed)
            {
                IsLoading = true;

                var switchData = new SceneSwitchData(gameSave.basic.stage, gameSave.basic.savePoint, false);

                UiHelpers.ForceCloseAllUi();

                Game.SwitchGameSave(gameSave);
                Game.SwitchScene(switchData);
            }
        });
    }

    public void LoadSaveState(SaveState saveState)
    {
        // Singletons.Dispatcher.Enqueue(() =>
        // {
        //     if (SceneUtils.IsLoading())
        //     {
        //         return;
        //     }
        //
        //     if (Game.ReadGameSave(index, out var gameSave) == ReadFileResult.Succeed)
        //     {
        //         IsLoading = true;
        //
        //         var switchData = new SceneSwitchData(gameSave.basic.stage, gameSave.basic.savePoint, false);
        //
        //         Game.SwitchGameSave(gameSave);
        //         Game.SwitchScene(switchData);
        //     }
        // });
    }

    private string GetGameSavePathFromIndex(int index) =>
        Path.Combine(NobetaSavesDirectory.FullName, $"GameSave{index:D2}.dat");
}