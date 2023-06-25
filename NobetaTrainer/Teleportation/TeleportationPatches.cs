using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NobetaTrainer.Utils;
using UnityEngine;

#if V1031
using Il2CppInterop.Runtime;
#endif

namespace NobetaTrainer.Teleportation;

public static class TeleportationPatches
{
    public static IEnumerable<SavePoint> SavePoints { get; private set; }
    public static IEnumerable<TreasureBox> TreasureBoxes { get; private set; }
    public static IEnumerable<AreaCheck> AreaChecks { get; private set; }

    public static string BuildingPointName = "";

    private static Action LastTeleportationAction;

    public static void TeleportToTarget(Transform targetTransform, Vector3 teleportationOffset, Quaternion rotationOffset)
    {
        IEnumerator Task()
        {
            TeleportTo(targetTransform.position, targetTransform.rotation, teleportationOffset, rotationOffset);

            // Find Scene where this object is defined
#if V1031
            var scene = targetTransform.gameObject.GetComponentInParent(Il2CppType.Of<SceneIsHide>()).gameObject;
#else
            var scene = targetTransform.gameObject.GetComponentInParent<SceneIsHide>().gameObject;
#endif

            yield return new WaitForEndOfFrame();

            // Find first AreaCheck that loads this Scene
            foreach (var areaCheck in AreaChecks)
            {
                if (areaCheck.ShowArea.Any(gameObject => gameObject.GetInstanceID() == scene.GetInstanceID()))
                {
                    areaCheck.OpenEvent();

                    break;
                }
            }

            ResetCamera();
        }

        Singletons.Dispatcher.Enqueue(Task());

        // Save for command use
        LastTeleportationAction = () => TeleportToTarget(targetTransform, teleportationOffset, rotationOffset);
    }

    public static void TeleportToPoint(TeleportationPoint teleportationPoint)
    {
        IEnumerator Task()
        {
            TeleportTo(teleportationPoint.Position, teleportationPoint.Rotation, Vector3.zero, Quaternion.identity);
            // Update camera rotation early to avoid character moving in wrong direction if a movement is done in this specific frame before the reset
            // This happens when you keep pressing a key on load for example
            Singletons.WizardGirl.GetCamera().g_CameraLookAt.rotation = teleportationPoint.Rotation;

            yield return new WaitForEndOfFrame();

            // Find AreaCheck with specified name and load associated areas
            var areaCheck = UnityUtils.FindComponentByNameForced<AreaCheck>(teleportationPoint.AreaCheckName);
            areaCheck.OpenEvent();

            ResetCamera();
        }

        Singletons.Dispatcher.Enqueue(Task());

        // Save for command use
        LastTeleportationAction = () => TeleportToPoint(teleportationPoint);
    }

    public static void TeleportTo(Vector3 position, Quaternion rotation, Vector3 teleportationOffset, Quaternion rotationOffset)
    {
        if (Singletons.WizardGirl?.transform is { } transform)
        {
            transform.position = position + teleportationOffset;
            transform.rotation = rotation * rotationOffset;
        }

        Singletons.Timers.ResetTeleportTimer();
    }

    public static void ResetCamera()
    {
        Singletons.WizardGirl.GetCamera().ResetCameraTeleport();
        Singletons.WizardGirl.GetCamera().CameraReset();
    }

    public static void TeleportLastPoint()
    {
        LastTeleportationAction?.Invoke();
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        SavePoints = UnityUtils.FindComponentsByTypeForced<SavePoint>().OrderBy(savePoint => savePoint.name);
        TreasureBoxes = UnityUtils.FindComponentsByTypeForced<TreasureBox>();
        AreaChecks = UnityUtils.FindComponentsByTypeForced<AreaCheck>();
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        SavePoints = null;
        TreasureBoxes = null;
        AreaChecks = null;
    }
}