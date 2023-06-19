using ImGuiNET;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;
using NobetaTrainer.Utils.Extensions;

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
            ImGui.TextColored(WarningColor, $"Loading...{new string('.', (_saveLoadPoints++) / 100)}");
        }
        else
        {
            _saveLoadPoints = 0;
        }

        WithDisabled(saveManager.IsLoading || SceneUtils.IsLoading(), () =>
        {
            ImGui.PushTextWrapPos();
            if (ImGui.CollapsingHeader("Game Saves", ImGuiTreeNodeFlags.DefaultOpen))
            {
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
            }

            if (ImGui.CollapsingHeader("Manage Save States"))
            {
                ImGui.SeparatorText("Create Save State");
                WithDisabled(Singletons.GameSave?.basic?.dataIndex is not (>= 1 and <= 9), () =>
                {
                    ImGui.InputText("Group##Create", ref saveManager.CreateSaveStateGroup, 100);
                    ImGui.InputText("Name##Create", ref saveManager.CreateSaveStateName, 100);
                    if (ImGui.Button("Create##SaveState"))
                    {
                        saveManager.CreateSaveState();
                    }
                    ImGui.SameLine();
                    ImGui.Checkbox("With Teleport", ref saveManager.CreateWithTeleport);
                });

                ImGui.SeparatorText("Modify Save State");
                var loadedSaveState = saveManager.LoadedSaveState;
                WithDisabled(loadedSaveState is null, () =>
                {
                    ImGui.TextColored(TitleColor, "Modify currently loaded save state (name in purple)");
                    ImGui.Separator();

                    ImGui.InputText("New Name##Label", ref saveManager.RenameSaveStateName, 100);

                    if (ButtonColored(PrimaryButtonColor, "Rename##Button"))
                    {
                        loadedSaveState!.SaveName = saveManager.RenameSaveStateName;
                        saveManager.Save();
                    }

                    ImGui.SameLine();
                    if (ButtonColored(ErrorButtonColor, "Delete"))
                    {
                        saveManager.DeleteSaveState(loadedSaveState);
                    }

                    ImGui.Separator();

                    ImGui.Combo("Group##Change", ref saveManager.ModifySaveStateGroupIndex, saveManager.GroupNames,saveManager.GroupNames.Length);

                    if (ImGui.Button("Change Group##Button"))
                    {
                        loadedSaveState!.GroupName = saveManager.GroupNames[saveManager.ModifySaveStateGroupIndex];
                        saveManager.Save();
                        saveManager.UpdateGroups();
                    }

                    ImGui.Separator();
                    ImGui.TextColored(InfoColor, "Teleportation point:");
                    ImGui.SameLine();
                    ImGui.TextColored(ValueColor, loadedSaveState?.TeleportationPoint?.Position.Format() ?? "N/A");

                    if (ImGui.Button("Update Teleport##TeleportationPoint"))
                    {
                        loadedSaveState?.UpdateTeleportationPoint();
                        saveManager.Save();
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Remove Teleport##TeleportationPoint"))
                    {
                        loadedSaveState?.RemoveTeleportationPoint();
                        saveManager.Save();
                    }
                });
            }

            if (ImGui.CollapsingHeader("Save states", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.TextColored(InfoColorSecondary, "There are");
                ImGui.SameLine();
                ImGui.TextColored(ValueColor, saveManager.SaveStatesCount.ToString());
                ImGui.SameLine();
                ImGui.TextColored(InfoColorSecondary, "save states available");
                WithDisabled(saveManager.IsExporting, () =>
                {
                    if (ButtonColored(TitleColor, "Import"))
                    {
                        saveManager.ImportGroup();
                    }
                });
                ImGui.NewLine();
                TabBar("SaveStatesTabBar", ImGuiTabBarFlags.AutoSelectNewTabs, () =>
                {
                    foreach (var group in saveManager.SaveStateGroups)
                    {
                        TabItem(group.Key, () =>
                        {
                            foreach (var saveState in group)
                            {
                                if (ImGui.Button($"Load##{saveState.Id}"))
                                {
                                    saveManager.LoadSaveState(saveState);
                                }

                                ImGui.SameLine(55);
                                ImGui.TextColored(
                                    ReferenceEquals(saveManager.LoadedSaveState, saveState) ? TitleColor : InfoColor,
                                    $"{saveState.SaveName}");

                                ImGui.Text("");
                                ImGui.SameLine(55);
                                ImGui.TextColored(WarningColor, ">");
                                ImGui.SameLine();
                                ImGui.TextColored(InfoColorSecondary, $"{saveState.StageName}");
                                ImGui.SameLine();
                                ImGui.TextColored(ValueColor, $"[{saveState.Difficulty} {saveState.ClearedCount}*]");
                            }

                            ImGui.SeparatorText("");
                            TreeNode("Group Operations", TitleColor, () =>
                            {
                                ImGui.InputText("New Name", ref saveManager.RenameGroupName, 100);

                                if (ButtonColored(PrimaryButtonColor, "Rename##Button"))
                                {
                                    saveManager.RenameGroup(group.Key);
                                }

                                ImGui.SeparatorText("");
                                WithDisabled(saveManager.IsExporting, () =>
                                {
                                    if (ButtonColored(TitleColor, "Export"))
                                    {
                                        saveManager.ExportGroup(group.Key);
                                    }
                                });
                                ImGui.SameLine();
                                if (ButtonColored(ErrorButtonColor, "Delete"))
                                {
                                    saveManager.DeleteGroup(group.Key);
                                }
                            });
                        });
                    }
                });
            }
        });
        ImGui.PopTextWrapPos();

        ImGui.End();
    }
}