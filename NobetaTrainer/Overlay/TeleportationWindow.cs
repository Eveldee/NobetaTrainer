using System;
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

        if (ImGui.CollapsingHeader("Save Points"))
        {
            foreach (var savePoint in TeleportationPatches.SavePoints)
            {
                ShowTeleportTarget($"{savePoint.name}_{savePoint.TransferLevelNumber}_{savePoint.TransferSavePointNumber}", savePoint.transform, Vector3.zero, Quaternion.identity);
            }
        }

        if (ImGui.CollapsingHeader("Chests"))
        {
            foreach (var treasureBox in TeleportationPatches.TreasureBoxes)
            {
                var rotation = treasureBox.transform.rotation.ToEulerAngles();
                var wantedDirection = rotation.y;
                var positionOffset = Quaternion.Euler(0f, wantedDirection * 360/(2 * (float)Math.PI), 0f) * new Vector3(0f, 0f, -.7f);

                Vector3.RotateTowards(positionOffset, rotation, -1, -1);

                ShowTeleportTarget($"{treasureBox.name}_{treasureBox.ItemType}_{(treasureBox.hasOpened ? "Opened" :  "Closed")}", treasureBox.transform, positionOffset, Quaternion.identity);
            }
        }

        if (ImGui.CollapsingHeader("Custom", ImGuiTreeNodeFlags.DefaultOpen))
        {

        }

        ImGui.End();
    }
}