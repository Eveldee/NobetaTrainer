using HarmonyLib;

namespace NobetaTrainer.Patches;

public class TitleSceneManagerPatches
{
    [HarmonyPatch(typeof(TitleSceneManager), nameof(TitleSceneManager.Enter))]
    [HarmonyPostfix]
    static void EnterPostfix()
    {
        // UiGameSavePatches.CurrentGameSave = null;
    }
}