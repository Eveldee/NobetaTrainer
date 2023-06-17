using System;
using ImGuiNET;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Overlay;

public partial class NobetaTrainerOverlay
{
    private int _saveLoadPoints = 0;

    private void ShowSavesWindow()
    {
        ImGui.Begin("Saves", ref OverlayState.ShowSavesWindow);

        if (Singletons.SavesManager is not { } saveManager)
        {
            ImGui.Text("Waiting for save manager to be loaded...");
            ImGui.End();

            return;
        }

        ImGui.TextColored(InfoColor, "Saves Manager");

        if (saveManager.IsLoading || SceneUtils.IsLoading())
        {
            ImGui.TextColored(WarningColor, $"Loading...{new string('.', (_saveLoadPoints++)/100)}");
        }
        else
        {
            _saveLoadPoints = 0;
        }

        ImGui.PushTextWrapPos();
        if (ImGui.CollapsingHeader("Game Saves", ImGuiTreeNodeFlags.DefaultOpen))
        {
            WithDisabled(saveManager.IsLoading || SceneUtils.IsLoading(), () =>
            {
                ImGui.SeparatorText("");
                foreach (var gameSaveInfo in saveManager.GameSaveInfos)
                {
                    if (ImGui.Button($"Load##{gameSaveInfo.Index}"))
                    {
                        saveManager.LoadGameSave(gameSaveInfo);
                    }

                    ImGui.SameLine(55);
                    ImGui.TextColored(
                        Singletons.GameSave?.basic?.dataIndex == gameSaveInfo.Index ? TitleColor : InfoColor,
                        $"({gameSaveInfo.Index:D2})"
                    );
                    ImGui.SameLine();
                    ImGui.TextColored(InfoColorSecondary, $"{gameSaveInfo.StageName}");
                    ImGui.SameLine();
                    ImGui.TextColored(ValueColor, $"[{gameSaveInfo.Difficulty} {gameSaveInfo.ClearedCount}*]");
                }
            });
        }

        if (ImGui.CollapsingHeader("Save states", ImGuiTreeNodeFlags.DefaultOpen))
        {
            WithDisabled(saveManager.IsLoading || SceneUtils.IsLoading(), () =>
            {
                ImGui.SeparatorText("");
                foreach (var saveState in saveManager.SaveStates)
                {
                    if (ImGui.Button($"Load##{saveState.Id}"))
                    {
                        saveManager.LoadSaveState(saveState);
                    }

                    ImGui.SameLine(55);
                    ImGui.TextColored(ReferenceEquals(saveManager.LoadedSaveState, saveState) ? TitleColor : InfoColor, $"{saveState.SaveName}");

                    ImGui.Text("");
                    ImGui.SameLine(55);
                    ImGui.TextColored(WarningColor, ">");
                    ImGui.SameLine();
                    ImGui.TextColored(InfoColorSecondary, $"{saveState.StageName}");
                    ImGui.SameLine();
                    ImGui.TextColored(ValueColor, $"[{saveState.Difficulty} {saveState.ClearedCount}*]");
                }

                ImGui.SeparatorText("Create Save State");
                WithDisabled(Singletons.GameSave?.basic?.dataIndex is not (>= 1 and <= 9), () =>
                {
                    ImGui.InputText("Name##Create", ref saveManager.CreateSaveStateName, 100);
                    if (ImGui.Button("Create##SaveState"))
                    {
                        saveManager.CreateSaveState();
                    }
                });

                ImGui.SeparatorText("Modify Save State");
                if (saveManager.LoadedSaveState is { } loadedSaveState)
                {
                    ImGui.InputText("Rename##Label", ref saveManager.RenameSaveStateName, 100);

                    if (ButtonColored(PrimaryButtonColor, "Rename##Button"))
                    {
                        loadedSaveState.SaveName = saveManager.RenameSaveStateName;
                    }
                    ImGui.SameLine();
                    if (ButtonColored(ErrorButtonColor, "Delete"))
                    {
                        saveManager.DeleteSaveState(loadedSaveState);
                    }
                }
            });
        }
        ImGui.PopTextWrapPos();

        ImGui.End();
    }
}