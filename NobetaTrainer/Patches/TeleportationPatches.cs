using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Patches;

public static class TeleportationPatches
{
    public static bool IsGameScene;
    public static IEnumerable<SavePoint> SavePoints { get; private set; }
    public static IEnumerable<TreasureBox> TreasureBoxes { get; private set; }

    public static void TeleportToTarget(Transform targetTransform, Vector3 teleporationOffset, Quaternion rotationOffset)
    {
        TeleportTo(targetTransform.position, targetTransform.rotation, teleporationOffset, rotationOffset);
    }

    public static void TeleportTo(Vector3 position, Quaternion rotation, Vector3 teleporationOffset, Quaternion rotationOffset)
    {
        if (Singletons.WizardGirl?.transform is { } transform)
        {
            transform.position = position + teleporationOffset;
            transform.rotation = rotation * rotationOffset;

            Singletons.WizardGirl.GetCamera().ResetCameraTeleport();
        }
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        Plugin.Log.LogDebug($"New scene init complete: {Game.sceneManager.stageName}");

        SavePoints = UnityUtils.FindComponentsByTypeForced<SavePoint>().OrderBy(savePoint => savePoint.name);
        TreasureBoxes = UnityUtils.FindComponentsByTypeForced<TreasureBox>();

        IsGameScene = true;
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        Plugin.Log.LogDebug("Entered loader scene");

        IsGameScene = false;

        SavePoints = null;
    }
}