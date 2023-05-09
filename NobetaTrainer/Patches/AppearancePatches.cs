using System;
using HarmonyLib;
using NobetaTrainer.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NobetaTrainer.Patches;

public static class AppearancePatches
{
    public static int SelectedSkinIndex;
    public static readonly string[] AvailableSkins = Enum.GetNames<GameSkin>();

    public static bool HideBagEnabled;
    public static bool HideStaffEnabled;
    public static bool HideHatEnabled;
    public static bool UseNobetaSkin;

    public static void LoadSelectedSkin()
    {
        if (Singletons.WizardGirl is not { } wizardGirlManage)
        {
            return;
        }

        var gameSkin = (GameSkin) SelectedSkinIndex;

        Singletons.Dispatcher.Enqueue(() =>
        {
            wizardGirlManage.PreloadSkin(gameSkin);
            var assetKey = wizardGirlManage.GetSkinAssetKey(gameSkin);

            // Need to keep the object in a variable to avoid getting GC'd before the call to ReplaceActiveSkin
            var _ = Addressables.LoadAsset<GameObject>(assetKey).WaitForCompletion();

            wizardGirlManage.ReplaceActiveSkin(gameSkin);

            // Also update skin in GameCollection for reload
            Game.Collection.UpdateSkin(gameSkin);
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
        });
    }

    public static void ToggleNobetaSkin()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            Singletons.WizardGirl.isNobeta = UseNobetaSkin;

            if (Singletons.WizardGirl.currentActiveSkin == GameSkin.Witch)
            {
                SelectedSkinIndex = 1;
                LoadSelectedSkin();
                SelectedSkinIndex = 0;
                LoadSelectedSkin();
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
}