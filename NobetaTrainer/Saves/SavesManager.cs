using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Il2CppSystem.IO;
using MarsSDK;
using NobetaTrainer.Config.Serialization;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;
using DirectoryInfo = System.IO.DirectoryInfo;
using File = System.IO.File;

namespace NobetaTrainer.Saves;

public class SavesManager
{
    public static readonly DirectoryInfo NobetaSavesDirectory = new(
        Path.Combine(Plugin.ConfigDirectory.FullName, "..", "..", "..", "LittleWitchNobeta_Data", "Save")
    );
    public static readonly DirectoryInfo SaveStatesDirectory = new(
        Path.Combine(Plugin.ConfigDirectory.FullName, "..", "..", "..", "LittleWitchNobeta_Data", "Save", "SaveStates")
    );

    public string SavePath { get; } = Path.Combine(Plugin.ConfigDirectory.FullName, "SaveStates.json");

    public const int SaveStateIndex = 9;

    public static string GetGameSavePathFromIndex(int index) =>
        Path.Combine(NobetaSavesDirectory.FullName, $"GameSave{index:D2}.dat");

    public static string GetGameSaveStatePathFromGuid(Guid guid) =>
        Path.Combine(SaveStatesDirectory.FullName, $"{guid}.dat");

    public IEnumerable<GameSaveInfo> GameSaveInfos => _gameSaveInfos;
    // TODO cache result
    // public IEnumerable<IGrouping<string, SaveState>> SaveStates => _saveStates
    //     .OrderBy(saveState => saveState.SaveName)
    //     .GroupBy(saveState => saveState.GroupName)
    //     .OrderBy(group => group.Key);
    public IEnumerable<IGrouping<string, SaveState>> SaveStateGroups = Enumerable.Empty<IGrouping<string, SaveState>>();
    public string[] GroupNames = Array.Empty<string>();

    public bool IsLoading { get; set; } = true;
    public string CreateSaveStateName = "Save State 01";
    public SaveState LoadedSaveState = null;

    private GameSaveInfo[] _gameSaveInfos;
    private List<SaveState> _saveStates;
    public string RenameSaveStateName = "New name";
    public string CreateSaveStateGroup = "New Group";
    public int ModifySaveStateGroupIndex;

    public SavesManager()
    {
        // Create SaveStates directory if needed
        if (!SaveStatesDirectory.Exists)
        {
            SaveStatesDirectory.Create();
        }

        // TODO Cleanup orphaned save states and missing save state files

        UpdateSaves();

        Load();
    }

    public void UpdateSaves()
    {
        _gameSaveInfos = Enumerable.Range(1, 9)
            .Where(index => File.Exists(GetGameSavePathFromIndex(index)))
            .Select(GameSaveInfo.FromSaveIndex)
            .ToArray();
    }

    public void UpdateGroups()
    {
        var groups = _saveStates
            .OrderBy(saveState => saveState.SaveName)
            .GroupBy(saveState => saveState.GroupName)
            .OrderBy(group => group.Key);

        SaveStateGroups = groups.ToArray();
        GroupNames = SaveStateGroups.Select(group => group.Key).OrderBy(name => name).ToArray();
    }

    public void LoadGameSave(GameSaveInfo gameSaveInfo)
    {
        LoadGameSave(gameSaveInfo.Index);
    }

    public void LoadGameSave(int gameSaveIndex)
    {
        if (gameSaveIndex is not (>= 1 and <= 9))
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            if (SceneUtils.IsLoading())
            {
                return;
            }

            if (Game.ReadGameSave(gameSaveIndex, out var gameSave) == ReadFileResult.Succeed)
            {
                IsLoading = true;

                var switchData = new SceneSwitchData(gameSave.basic.stage, gameSave.basic.savePoint, false);

                UiHelpers.ForceCloseAllUi();

                Game.SwitchGameSave(gameSave);
                Game.SwitchScene(switchData);
            }
        });

        LoadedSaveState = null;
    }

    public void LoadSaveState(SaveState saveState)
    {
        if (SceneUtils.IsLoading())
        {
            return;
        }

        var sourcePath = GetGameSaveStatePathFromGuid(saveState.Id);
        var destinationPath = GetGameSavePathFromIndex(SaveStateIndex);

        if (!File.Exists(sourcePath))
        {
            Plugin.Log.LogError($"Tried to load save state at location '{sourcePath}' but found no file");
            return;
        }

        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        // Copy save state to SaveStateIndex
        File.Copy(sourcePath, destinationPath!);

        // Load this index
        LoadGameSave(SaveStateIndex);

        LoadedSaveState = saveState;
        RenameSaveStateName = saveState.SaveName;
        CreateSaveStateGroup = saveState.GroupName;
        ModifySaveStateGroupIndex = Array.IndexOf(GroupNames, saveState.GroupName);
        UpdateSaves();
    }

    public void CreateSaveState()
    {
        if (CreateSaveStateName.IsNullOrWhiteSpace())
        {
            return;
        }

        if (SaveState.TryCreateFromCurrentSave(CreateSaveStateName, CreateSaveStateGroup, out var saveState))
        {
            _saveStates.Add(saveState);
            LoadedSaveState = saveState;
            RenameSaveStateName = saveState.SaveName;

            Save();
            UpdateGroups();
        }
    }

    public void DeleteSaveState(SaveState loadedSaveState)
    {
        _saveStates.Remove(loadedSaveState);
        LoadedSaveState = null;

        Save();
        UpdateGroups();
    }

    public void Save()
    {
        File.WriteAllText(SavePath, SerializeUtils.SerializeIndented(_saveStates));
    }

    private void Load()
    {
        if (!File.Exists(SavePath))
        {
            _saveStates = new List<SaveState>();

            return;
        }

        _saveStates = SerializeUtils.Deserialize<List<SaveState>>
        (
            File.ReadAllText(SavePath)
        );

        UpdateGroups();
    }
}