using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NobetaTrainer.Config.Models;
using NobetaTrainer.Prefabs;
using NobetaTrainer.Utils;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Patches;

public static class CollidersRenderPatches
{
    public static bool ShowColliders;
    public static GameObject RenderersContainer;

    private static readonly List<BoxColliderRenderer> _boxColliderRenderers = new();

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
        foreach (var sceneEvent in UnityUtils.FindComponentsByTypeForced<SceneEvent>())
        {
            if (sceneEvent is AreaCheck or MagicWall or LoadScript)
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
        // foreach (var colliderRenderer in _boxColliderRenderers)
        // {
        //     colliderRenderer.Destroy();
        // }

        _boxColliderRenderers.Clear();
    }
}