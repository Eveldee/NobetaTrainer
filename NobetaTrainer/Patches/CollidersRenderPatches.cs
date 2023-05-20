using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NobetaTrainer.Prefabs;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Patches;

public static class CollidersRenderPatches
{
    public static bool DrawLines;

    private static readonly List<BoxColliderRenderer> _boxColliderRenderers = new();

    public static void UpdateDrawLines()
    {
        foreach (var boxColliderRenderer in _boxColliderRenderers)
        {
            boxColliderRenderer.SetVisible(DrawLines);
        }
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        // Draw AreaCheck colliders
        // foreach (var areaCheck in UnityUtils.FindComponentsByTypeForced<AreaCheck>())
        // {
        //     if (areaCheck.g_BC is not null)
        //     {
        //         _boxColliderRenderers.Add(new BoxColliderRenderer($"{areaCheck.name}-ColliderRenderer", areaCheck.transform, areaCheck.g_BC));
        //     }
        // }
        foreach (var boxCollider in UnityUtils.FindComponentsByTypeForced<BoxCollider>())
        {
            _boxColliderRenderers.Add(new BoxColliderRenderer($"{boxCollider.name}-ColliderRenderer", boxCollider.transform, boxCollider));
        }

        UpdateDrawLines();
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        foreach (var colliderRenderer in _boxColliderRenderers)
        {
            colliderRenderer.Destroy();
        }

        _boxColliderRenderers.Clear();
    }
}