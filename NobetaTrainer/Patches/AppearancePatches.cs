using HarmonyLib;
using NobetaTrainer.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace NobetaTrainer.Patches;

public static class AppearancePatches
{
    public static int SelectedSkinIndex;

    public static bool HideBagEnabled;
    public static bool HideStaffEnabled;
    public static bool HideHatEnabled;

    private static NobetaSkin _currentSkin;

    public static void LoadSelectedSkin()
    {
        if (WizardGirlManagePatches.Instance is not { } wizardGirlManage)
        {
            return;
        }

        var gameSkin = (GameSkin) SelectedSkinIndex;

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            wizardGirlManage.PreloadSkin(gameSkin);
            var assetKey = wizardGirlManage.GetSkinAssetKey(gameSkin);

            // Need to keep the object in a variable to avoid getting GC'd before the call to ReplaceActiveSkin
            var nobetaSkin = Addressables.LoadAsset<GameObject>(assetKey).WaitForCompletion();

            wizardGirlManage.ReplaceActiveSkin(gameSkin);

            // Also update skin in GameCollection for reload
            Game.Collection.UpdateSkin(gameSkin);
        });
    }

    public static void UpdateAppearance()
    {
        if (_currentSkin is not { } skin)
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
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.UpdateSkin))]
    [HarmonyPostfix]
    private static void PlayerControllerUpdateSkinPostfix(PlayerController __instance, NobetaSkin skin)
    {
        _currentSkin = skin;

        UpdateAppearance();
    }

    [HarmonyPatch(typeof(PlayerController), nameof(NobetaSkin.Dispose))]
    [HarmonyPostfix]
    private static void NobetaSkinDispose()
    {
        _currentSkin = null;
    }

    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.StartGamePlay))]
    [HarmonyPostfix]
    static void StartGamePlayPostfix()
    {
        SelectedSkinIndex = (int) Game.Collection.currentSkin;
    }
}