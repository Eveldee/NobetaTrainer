using HarmonyLib;
using UnityEngine;

namespace NobetaTrainer;

public class WizardGirlManagePatches
{
    public static WizardGirlManage Instance;
    public static NobetaRuntimeData RuntimeData;

    private static bool _appliedNoDamage;

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    [HarmonyPostfix]
    static void WizardGirlManageInitPostfix(WizardGirlManage __instance)
    {
        Plugin.Log.LogDebug($"New instance of {nameof(WizardGirlManage)} created");
        Instance = __instance;
        RuntimeData = Instance.playerController.runtimeData;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Hit))]
    [HarmonyPrefix]
    static bool HitPrefix(ref bool __result, AttackData Data, bool bIgnoreDodge = false)
    {
        // No Damage
        return !Plugin.TrainerOverlay.NoDamageEnabled;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    [HarmonyPrefix]
    static void UpdatePrefix()
    {

    }
}