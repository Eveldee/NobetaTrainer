using System;
using HarmonyLib;

namespace NobetaTrainer.Patches;

public static class TimersPatches
{
    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Appear))]
    [HarmonyPrefix]
    private static void UIPauseMenuAppearPrefix()
    {
        Singletons.Timers.Pause();
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Hide))]
    [HarmonyPostfix]
    private static void UIPauseMenuHidePostfix()
    {
        Singletons.Timers.Resume();
    }

    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Move))]
    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Jump))]
    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Dodge))]
    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Attack))]
    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Shoot))]
    [HarmonyPostfix]
    private static void PlayerInputControllerAnyPostfix()
    {
        if (Singletons.WizardGirl is { } wizardGirl && wizardGirl.playerController.CharacterControllable)
        {
            Singletons.Timers.Resume();
        }
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    [HarmonyPostfix]
    private static void WizardGirlInitPostfix()
    {
        Singletons.Timers.ResetLoadTimer();
    }

    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave), new Type[]{ })]
    [HarmonyPostfix]
    public static void WriteGameSavePostfix()
    {
        Singletons.Timers.ResetSaveTimer();
    }
}