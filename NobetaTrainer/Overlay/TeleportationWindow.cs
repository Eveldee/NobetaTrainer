using System.Collections;
using ImGuiNET;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay
{
    private void ShowTeleportationWindow()
    {
        void ShowTeleportTarget(string name, Transform targetObject, Vector3 teleportationOffset, Quaternion rotationOffset)
        {
            if (ImGui.Button($"Teleport##{targetObject.name}"))
            {
                Singletons.Dispatcher.Enqueue(() => TeleportationPatches.TeleportToTarget(targetObject, teleportationOffset, rotationOffset));
            }
            ImGui.SameLine();
            ImGui.TextColored(InfoColor, name);
            ImGui.SameLine();
            ImGui.TextColored(ValueColor, targetObject.position.Format());
        }

        ImGui.Begin("Teleportation", ref OverlayState.ShowTeleportationWindow);

        if (!TeleportationPatches.IsGameScene)
        {
            ImGui.Text("Waiting for a scene to be loaded...");

            ImGui.End();
            return;
        }

        ImGui.TextColored(InfoColor, "Teleportation points for stage:");
        ImGui.SameLine();
        ImGui.TextColored(ValueColor, $"{Game.sceneManager.stageName}");

        if (ImGui.CollapsingHeader("Save Points", ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach (var savePoint in TeleportationPatches.SavePoints)
            {
                ShowTeleportTarget($"{savePoint.name}_{savePoint.TransferLevelNumber}_{savePoint.TransferSavePointNumber}", savePoint.transform, Vector3.zero, Quaternion.identity);
            }
        }

        if (ImGui.CollapsingHeader("Chests"))
        {

        }

        if (ImGui.CollapsingHeader("Custom"))
        {

        }

        ImGui.End();
    }
}