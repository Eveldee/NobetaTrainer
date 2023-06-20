using System;
using System.Linq;
using BepInEx;
using ImGuiNET;
using NobetaTrainer.Teleportation;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;
using NobetaTrainer.Utils.Extensions;
using UnityEngine;

namespace NobetaTrainer.Overlay;

public partial class NobetaTrainerOverlay
{
    private void ShowTeleportationWindow()
    {
        void ShowTeleportTarget(string name, Transform targetObject, Vector3 teleportationOffset, Quaternion rotationOffset)
        {
            if (ImGui.Button($"Teleport##{targetObject.name}"))
            {
                TeleportationPatches.TeleportToTarget(targetObject, teleportationOffset, rotationOffset);
            }
            ImGui.SameLine();
            ImGui.TextColored(InfoColor, name);
        }
        void ShowTeleportPoint(TeleportationPoint teleportationPoint, int index)
        {
            if (ImGui.Button($"Teleport##{index}"))
            {
                TeleportationPatches.TeleportToPoint(teleportationPoint);
            }
            ImGui.SameLine();
            if (ButtonColored(ErrorButtonColor, $"Delete##{index}"))
            {
                Singletons.TeleportationManager.RemovePoint(teleportationPoint);
            }
            ImGui.SameLine();
            ImGui.TextColored(InfoColor, teleportationPoint.PointName);
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
            int index = 0;
            foreach (var teleportationPoint in Singletons.TeleportationManager.TeleportationPoints.ToArray())
            {
                ShowTeleportPoint(teleportationPoint, index++);
            }
        }

        if (ImGui.CollapsingHeader("Create", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Point builder
            var transform = Singletons.WizardGirl.transform;

            ShowValue(InfoColorSecondary, "Position:", transform.position.Format());
            ShowValue(InfoColorSecondary, "Rotation:", transform.rotation.Format());

            ImGui.NewLine();
            ImGui.InputText("Name", ref TeleportationPatches.BuildingPointName, 40);
            if (ImGui.Button("Create##Button") && !TeleportationPatches.BuildingPointName.IsNullOrWhiteSpace())
            {
                var areaCheck = SceneUtils.FindLastAreaCheck();

                Singletons.TeleportationManager.AddPoint(new TeleportationPoint(
                    TeleportationPatches.BuildingPointName,
                    transform.position,
                    transform.rotation,
                    areaCheck.name
                ));
            }
        }

        ImGui.End();
    }
}