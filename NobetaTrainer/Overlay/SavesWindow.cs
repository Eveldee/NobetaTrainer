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
                foreach (var gameSaveInfo in saveManager.GameSaveInfos)
                {
                    if (ImGui.Button($"Load##{gameSaveInfo.Index}"))
                    {
                        saveManager.LoadGameSave(gameSaveInfo);
                    }

                    ImGui.SameLine();
                    ImGui.TextColored(
                        Singletons.GameSave?.basic?.dataIndex == gameSaveInfo.Index ? TitleColor : InfoColor,
                        $"({gameSaveInfo.Index:D2})"
                    );
                    ImGui.SameLine();
                    ImGui.TextColored(InfoColorSecondary, $"{gameSaveInfo.StageName}");
                    ImGui.SameLine();
                    ImGui.TextColored(ValueColor, $"[{gameSaveInfo.Difficulty} {gameSaveInfo.ClearedCount}*]");
                    ImGui.SameLine();
                    ImGui.TextColored(WarningColor, $"{gameSaveInfo.LastSaveTimestamp}");
                }
            });
        }

        if (ImGui.CollapsingHeader("Save states", ImGuiTreeNodeFlags.DefaultOpen))
        {
            WithDisabled(saveManager.IsLoading || SceneUtils.IsLoading(), () =>
            {
                foreach (var saveState in saveManager.SaveStates)
                {
                    // if (ImGui.Button($"Load##{gameSaveInfo.Index}"))
                    // {
                    //     saveManager.LoadGameSave(gameSaveInfo);
                    // }
                    //
                    // ImGui.SameLine();
                    // ImGui.TextColored(
                    //     Singletons.GameSave?.basic?.dataIndex == gameSaveInfo.Index ? TitleColor : InfoColor,
                    //     $"({gameSaveInfo.Index:D2})"
                    // );
                    // ImGui.SameLine();
                    // ImGui.TextColored(InfoColorSecondary, $"{gameSaveInfo.StageName}");
                    // ImGui.SameLine();
                    // ImGui.TextColored(ValueColor, $"[{gameSaveInfo.Difficulty} {gameSaveInfo.ClearedCount}*]");
                    // ImGui.SameLine();
                    // ImGui.TextColored(WarningColor, $"{gameSaveInfo.LastSaveTimestamp}");
                }
            });
        }
        ImGui.PopTextWrapPos();

        ImGui.End();
    }
}