using HarmonyLib;
using UnityEngine;

namespace NobetaTrainer;

public class WizardGirlManagePatches
{
    public static WizardGirlManage Instance;

    private static bool _appliedNoDamage;

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    [HarmonyPostfix]
    static void WizardGirlManageInitPostfix(WizardGirlManage __instance)
    {
        Plugin.Log.LogDebug($"New instance of {nameof(WizardGirlManage)} created");
        Instance = __instance;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    [HarmonyPrefix]
    static void UpdatePrefix()
    {
        if (Plugin.TrainerOverlay.NoDamageEnabled && !_appliedNoDamage)
        {
            Instance.PlayerController.SetDodgeTime(float.MaxValue);

            _appliedNoDamage = true;
        }
        else
        {
            if (_appliedNoDamage)
            {
                Instance.playerController.SetDodgeTime();

                _appliedNoDamage = false;
            }
        }
    }
}