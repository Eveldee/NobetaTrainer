using HarmonyLib;

namespace NobetaTrainer.Patches;

public class UiGameSavePatches
{
    public static GameSave CurrentGameSave { get; set; }
    public static bool SaveLoaded => CurrentGameSave is not null;

    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.StartGamePlay))]
    [HarmonyPostfix]
    static void StartGamePlayPostfix(GameSave gameSave)
    {
        Plugin.Log.LogInfo("Save loaded");
        Plugin.TrainerOverlay.ForceShowTeleportMenu = Game.GameSave.basic.showTeleportMenu;

        CurrentGameSave = gameSave;
    }
}