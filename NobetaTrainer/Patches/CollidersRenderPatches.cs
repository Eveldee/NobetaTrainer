using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using NobetaTrainer.Config.Models;
using NobetaTrainer.Prefabs;
using NobetaTrainer.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Type = Il2CppSystem.Type;

namespace NobetaTrainer.Patches;

public static class CollidersRenderPatches
{
    public static bool ShowColliders;
    public static GameObject RenderersContainer;
    public static IGrouping<string, SceneEvent>[] CollidingSceneEvents;

    private static readonly List<BoxColliderRenderer> _boxColliderRenderers = new();
    private static Il2CppArrayBase<SceneEvent> _sceneEvents;

    public static void ToggleShowColliders()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            RenderersContainer.SetActive(ShowColliders);
        });
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

        RenderersContainer = new GameObject("ColliderRenderer_Container");
        RenderersContainer.SetActive(true);

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
        foreach (var boxCollider in UnityUtils.FindComponentsByTypeForced<BoxCollider>())
        {
            if (boxCollider.GetComponent<SceneEvent>() != null)
            {
                continue;
            }

            AddRenderer(boxCollider.transform, boxCollider, ColliderType.Other);
        }

        UpdateDrawLines(ColliderType.AreaCheck);
        UpdateDrawLines(ColliderType.SceneEvent);
        UpdateDrawLines(ColliderType.Other);
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        Object.Destroy(RenderersContainer);

        _boxColliderRenderers.Clear();
        _sceneEvents = null;
        CollidingSceneEvents = null;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    [HarmonyPrefix]
    private static void WizardGirlManageUpdatePostfix(WizardGirlManage __instance)
    {
        if (_sceneEvents == null)
        {
            return;
        }

        // Get all scene events that Nobeta is colliding with
        var nobetaPosition = __instance.g_PlayerCenter.position;

        CollidingSceneEvents = _sceneEvents
            .Where(sceneEvent => sceneEvent.g_BC != null && sceneEvent.g_BC.ContainsBox(nobetaPosition))
            .GroupBy(sceneEvent => sceneEvent.GetIl2CppType().Name).ToArray();
    }
}