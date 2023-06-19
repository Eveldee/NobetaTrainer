using System;
using HarmonyLib;
using NobetaTrainer.Teleportation;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Saves;

public static class SavePatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.SwitchTitleScene))]
    [HarmonyPostfix]
    private static void SwitchTitleScenePostfix()
    {
        Singletons.SavesManager = new SavesManager();
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        if (Singletons.SavesManager is { } savesManager)
        {
            // Teleport when needed
            if (savesManager.NeedTeleportationOnLoad)
            {
                savesManager.NeedTeleportationOnLoad = false;

                TeleportationPatches.TeleportToPoint(savesManager.LoadedSaveState.TeleportationPoint);
            }

            savesManager.IsLoading = false;
        }
    }

    [HarmonyPatch(typeof(TitleSceneManager), nameof(TitleSceneManager.Enter))]
    [HarmonyPostfix]
    private static void TitleSceneManagerEnterPostfix()
    {
        if (Singletons.SavesManager is { } savesManager)
        {
            savesManager.IsLoading = false;
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave), new Type[]{ })]
    [HarmonyPostfix]
    public static void WriteGameSavePostfix()
    {
        Singletons.SavesManager?.UpdateSaves();
    }

    // Stand up
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.SetResurrection))  ]
    [HarmonyPrefix]
    public static bool WizardSetResurrectionPrefix()
    {
        Plugin.Log.LogDebug("Nobeta resurrected");

        return Singletons.SavesManager is not { NeedTeleportationOnLoad: true };
    }
}