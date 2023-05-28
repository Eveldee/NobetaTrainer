using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Humanizer;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem;
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

    private static readonly IOrderedDictionary<Type, ColliderType> _componentToColliderType = new OrderedDictionary<Type, ColliderType>()
    {
        { Il2CppType.Of<AreaCheck>(), ColliderType.AreaCheck },
        { Il2CppType.Of<MagicWall>(), ColliderType.MagicWall },
        { Il2CppType.Of<LoadScript>(), ColliderType.LoadScript },
        { Il2CppType.Of<SceneEvent>(), ColliderType.SceneEvent }
    };

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
        RenderersContainer.SetActive(false);

        _sceneEvents = UnityUtils.FindComponentsByTypeForced<SceneEvent>();

        if (EnableOtherColliders)
        {
            foreach (var boxCollider in UnityUtils.FindComponentsByTypeForced<BoxCollider>())
            {
                var newColliderType = ColliderType.Other;

                foreach (var (componentType, colliderType) in _componentToColliderType)
                {
                    if (boxCollider.GetComponent(componentType) is not null)
                    {
                        newColliderType = colliderType;

                        break;
                    }
                }

                // Skip loading other colliders if they are disabled to avoid performance issues
                if (newColliderType == ColliderType.Other)
                {
                    if (!EnableOtherColliders)
                    {
                        continue;
                    }
                }

                AddRenderer(boxCollider.transform, boxCollider, newColliderType);
            }
        }

        foreach (var boxColliderRenderer in _boxColliderRenderers)
        {
            boxColliderRenderer.UpdateDisplay();
        }

        RenderersContainer.SetActive(ShowColliders);
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