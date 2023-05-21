using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using NobetaTrainer.Prefabs;
using NobetaTrainer.Utils;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Patches;

public static class CollidersRenderPatches
{
    public static bool DrawLines;
    public static float LineWidth = 0.05f;
    public static Vector4 LineStartColor = Color.blue.ToVector4();
    public static Vector4 LineEndColor = Color.red.ToVector4();

    private static readonly List<BoxColliderRenderer> _boxColliderRenderers = new();

    public static void UpdateDrawLines()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            var startColor = LineStartColor.ToColor();
            var endColor = LineEndColor.ToColor();

            foreach (var boxColliderRenderer in _boxColliderRenderers)
            {
                boxColliderRenderer.SetVisible(DrawLines);
                boxColliderRenderer.SetLineWidth(LineWidth);
                boxColliderRenderer.SetLineColor(startColor, endColor);
            }
        });
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        // Draw AreaCheck colliders
        foreach (var areaCheck in UnityUtils.FindComponentsByTypeForced<AreaCheck>())
        {
            if (areaCheck.g_BC is not null)
            {
                _boxColliderRenderers.Add(new BoxColliderRenderer($"{areaCheck.name}-ColliderRenderer", areaCheck.transform, areaCheck.g_BC));
            }
        }
        // foreach (var boxCollider in UnityUtils.FindComponentsByTypeForced<BoxCollider>())
        // {
        //     _boxColliderRenderers.Add(new BoxColliderRenderer($"{boxCollider.name}-ColliderRenderer", boxCollider.transform, boxCollider));
        // }

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