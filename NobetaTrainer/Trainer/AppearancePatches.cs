using System;
using System.Linq;
using EnumsNET;
using HarmonyLib;
using NobetaTrainer.Config;
using UnityEngine;

#if !V1031
using UnityEngine.AddressableAssets;
#endif

namespace NobetaTrainer.Trainer;

[Section("Character.Appearance")]
public static class AppearancePatches
{
    public static int SelectedSkinIndex;
    public static readonly string[] AvailableSkins = Enum.GetNames<GameSkin>();

    [Bind]
    public static bool HideBagEnabled;
    [Bind]
    public static bool HideStaffEnabled;
    [Bind]
    public static bool HideHatEnabled;
    [Bind]
    public static bool UseNobetaMoveset;
    [Bind]
    public static bool InvisibleEnabled;

    public static void LoadSelectedSkin()
    {
        if (Singletons.WizardGirl is not { } wizardGirlManage)
        {
            return;
        }

        var gameSkin = (GameSkin) SelectedSkinIndex;

        Singletons.Dispatcher.Enqueue(() =>
        {
            #if V1031
            wizardGirlManage.UpdateSkin(gameSkin);

            // Also update skin in GameCollection for reload
            Game.Collection.UpdateSkin(gameSkin);
            #else
            wizardGirlManage.PreloadSkin(gameSkin);
            var assetKey = wizardGirlManage.GetSkinAssetKey(gameSkin);

            // Need to keep the object in a variable to avoid getting GC'd before the call to ReplaceActiveSkin
            var _ = Addressables.LoadAsset<GameObject>(assetKey).WaitForCompletion();

            wizardGirlManage.ReplaceActiveSkin(gameSkin);

            // Also update skin in GameCollection for reload
            Game.Collection.UpdateSkin(gameSkin);
            #endif
        });
    }

    // Skin loader, hide bag, staff and hat
    public static void UpdateAppearance()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            if (Singletons.NobetaSkin is not { } skin)
            {
                return;
            }

            if (skin.bagMesh is not null)
            {
                skin.bagMesh.enabled = !HideBagEnabled;
            }

            if (skin.weaponMesh is not null)
            {
                skin.weaponMesh.enabled = !HideStaffEnabled;
            }

            if (skin.storyHatMesh is not null)
            {
                skin.storyHatMesh.enabled = !HideHatEnabled;
            }

            ToggleInvisible();
        });
    }

    public static void ToggleNobetaSkin()
    {
        if (Singletons.WizardGirl is null)
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            Singletons.WizardGirl.isNobeta = UseNobetaMoveset;

            var originalSkin = SelectedSkinIndex;

            SelectedSkinIndex = (originalSkin + 1) % Enums.GetMemberCount<GameSkin>();
            LoadSelectedSkin();
            SelectedSkinIndex = originalSkin;
            LoadSelectedSkin();
        });
    }

    public static void ToggleInvisible()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            if (Singletons.WizardGirl is not { } wizardGirl)
            {
                return;
            }

            var skin = wizardGirl.Skin;
            var renderers = skin.GetComponentsInChildren<MeshRenderer>(true)
                .Concat<Renderer>(skin.GetComponentsInChildren<SkinnedMeshRenderer>());

            foreach (var renderer in renderers)
            {
                if (renderer is not null)
                {
                    renderer.enabled = !InvisibleEnabled;
                }
            }
        });
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.UpdateSkin))]
    [HarmonyPostfix]
    private static void PlayerControllerUpdateSkinPostfix(PlayerController __instance, NobetaSkin skin)
    {
        UpdateAppearance();
    }

    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.StartGamePlay))]
    [HarmonyPostfix]
    private static void StartGamePlayPostfix()
    {
        SelectedSkinIndex = (int) Game.Collection.currentSkin;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    [HarmonyPrefix]
    private static void WizardGirlInitPrefix(WizardGirlManage __instance)
    {
        __instance.isNobeta = UseNobetaMoveset;
    }
}