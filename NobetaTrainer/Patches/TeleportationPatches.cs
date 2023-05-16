using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NobetaTrainer.Config;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Patches;

public static class TeleportationPatches
{
    public static bool IsGameScene;
    public static IEnumerable<SavePoint> SavePoints { get; private set; }
    public static IEnumerable<TreasureBox> TreasureBoxes { get; private set; }
    public static IEnumerable<AreaCheck> AreaChecks { get; private set; }

    public static string BuildingPointName = "";

    public static void TeleportToTarget(Transform targetTransform, Vector3 teleporationOffset, Quaternion rotationOffset)
    {
        TeleportTo(targetTransform.position, targetTransform.rotation, teleporationOffset, rotationOffset);

        // Find Scene where this object is defined
        var scene = targetTransform.gameObject.GetComponentInParent<SceneIsHide>().gameObject;

        // Find first AreaCheck that loads this Scene
        foreach (var areaCheck in AreaChecks)
        {
            if (areaCheck.ShowArea.Any(gameObject => gameObject.GetInstanceID() == scene.GetInstanceID()))
            {
                areaCheck.OpenEvent();

                break;
            }
        }
    }

    public static void TeleportToPoint(TeleportationPoint teleportationPoint)
    {
        TeleportTo(teleportationPoint.Position, teleportationPoint.Rotation, Vector3.zero, Quaternion.identity);

        // Find AreaCheck with specified name and load associated areas
        var areaCheck = UnityUtils.FindComponentByNameForced<AreaCheck>(teleportationPoint.AreaCheckName);
        areaCheck.OpenEvent();
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
        AreaChecks = UnityUtils.FindComponentsByTypeForced<AreaCheck>();

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