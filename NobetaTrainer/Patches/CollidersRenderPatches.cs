using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Humanizer;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using NobetaTrainer.Config;
using NobetaTrainer.Config.Models;
using NobetaTrainer.Prefabs;
using NobetaTrainer.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NobetaTrainer.Patches;

[Section("Colliders")]
public static class CollidersRenderPatches
{
    [Bind]
    public static bool EnableOtherColliders;
    [Bind]
    public static bool ShowColliders;

    public static GameObject RenderersContainer;
    public static IGrouping<string, SceneEvent>[] CollidingSceneEvents;
    public static IGrouping<string, Collider>[] CollidingColliders;

    private static readonly List<BoxColliderRenderer> _boxColliderRenderers = new();
    private static Il2CppArrayBase<SceneEvent> _sceneEvents;

    public static void ToggleShowColliders()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            RenderersContainer.SetActive(ShowColliders);
        });
        UpdateDrawLines(ColliderType.Other);
    }

    public static void UpdateDrawLines(ColliderType colliderType)
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            foreach (var boxColliderRenderer in _boxColliderRenderers.Where(renderer => renderer.ColliderType == colliderType))
            {
                boxColliderRenderer.UpdateDisplay();
            }
        });
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        void AddRenderer(Transform parent, BoxCollider boxCollider, ColliderType colliderType)
        {
            _boxColliderRenderers.Add(new BoxColliderRenderer($"{parent.name}-ColliderRenderer", parent, boxCollider, Singletons.ColliderRendererManager.RendererConfigs[colliderType], colliderType));
        }

        RenderersContainer = new GameObject("ColliderRenderer_RootContainer");
        RenderersContainer.SetActive(ShowColliders);

        foreach (var areaCheck in UnityUtils.FindComponentsByTypeForced<AreaCheck>())
        {
            if (areaCheck.g_BC is not null)
            {
                AddRenderer(areaCheck.transform, areaCheck.g_BC, ColliderType.AreaCheck);
            }
        }
        foreach (var magicWall in UnityUtils.FindComponentsByTypeForced<MagicWall>())
        {
            if (magicWall.g_BC is not null)
            {
                AddRenderer(magicWall.transform, magicWall.g_BC, ColliderType.MagicWall);
            }
        }
        foreach (var loadScript in UnityUtils.FindComponentsByTypeForced<LoadScript>())
        {
            if (loadScript.g_BC is not null)
            {
                AddRenderer(loadScript.transform, loadScript.g_BC, ColliderType.LoadScript);
            }
        }

        _sceneEvents = UnityUtils.FindComponentsByTypeForced<SceneEvent>();

        foreach (var sceneEvent in _sceneEvents)
        {
            if (sceneEvent.GetComponent<AreaCheck>() is not null || sceneEvent.GetComponent<MagicWall>() is not null || sceneEvent.GetComponent<LoadScript>() is not null)
            {
                continue;
            }

            if (sceneEvent.g_BC is not null)
            {
                AddRenderer(sceneEvent.transform, sceneEvent.g_BC, ColliderType.SceneEvent);
            }
        }
        // Skip loading other colliders if they are disabled to avoid performance issues
        if (EnableOtherColliders)
        {
            foreach (var boxCollider in UnityUtils.FindComponentsByTypeForced<BoxCollider>())
            {
                if (boxCollider.GetComponent<SceneEvent>() != null)
                {
                    continue;
                }

                AddRenderer(boxCollider.transform, boxCollider, ColliderType.Other);
            }
        }

        foreach (var boxColliderRenderer in _boxColliderRenderers)
        {
            boxColliderRenderer.UpdateDisplay();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        Object.Destroy(RenderersContainer);

        _boxColliderRenderers.Clear();
        _sceneEvents = null;
        CollidingSceneEvents = null;
        CollidingColliders = null;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    [HarmonyPrefix]
    private static void WizardGirlManageUpdatePostfix(WizardGirlManage __instance)
    {
        if (_sceneEvents == null)
        {
            return;
        }

        // Get all scene events that Nobeta center point is colliding with
        var worldNobetaPosition = __instance.g_PlayerCenter.position;

        CollidingSceneEvents = _sceneEvents
            .Where(sceneEvent => sceneEvent.g_BC != null && sceneEvent.g_BC.Contains(worldNobetaPosition))
            .GroupBy(sceneEvent => sceneEvent.GetIl2CppType().Name).ToArray();

        // Get all active colliders that Nobeta is exactly colliding with
        var capsule = __instance.characterController;
        var cylinderHeightVector = new Vector3(0, capsule.height / 2 - capsule.radius, 0);
        var capsuleCenter = __instance.transform.TransformPoint(capsule.center);

        CollidingColliders = Physics.OverlapCapsule(capsuleCenter - cylinderHeightVector, capsuleCenter + cylinderHeightVector, capsule.radius)
            .GroupBy(collider => collider.GetIl2CppType().Name).ToArray();
    }
}