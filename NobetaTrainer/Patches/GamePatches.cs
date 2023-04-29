using HarmonyLib;

namespace NobetaTrainer.Patches;

public class GamePatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.SwitchTitleScene))]
    [HarmonyPostfix]
    static void SwitchTitleScenePostfix()
    {
        UiGameSavePatches.CurrentGameSave = null;
    }
}