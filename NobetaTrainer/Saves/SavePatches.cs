using HarmonyLib;
using NobetaTrainer.Trainer;

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
}