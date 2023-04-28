using HarmonyLib;

namespace NobetaTrainer.Patches;

public class WizardGirlManagePatches
{
    public static WizardGirlManage Instance { get; private set; }
    public static NobetaRuntimeData RuntimeData { get; private set; }

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
        // Infinite Mana
        if (Plugin.TrainerOverlay.InfiniteManaEnabled)
        {
            var player = Instance;
            var data = player.BaseData;

            // Increase mana if needed
            if (data.g_fMP < data.g_fMPMax)
            {
                data.g_fMP = data.g_fMPMax;
            }
        }

        // Infinite Stamina
        if (Plugin.TrainerOverlay.InfiniteStaminaEnabled)
        {
            var player = Instance;
            var data = player.BaseData;

            // Increase stamina if needed
            if (data.g_fSP < data.g_fSPMax)
            {
                data.g_fSP = data.g_fSPMax;
            }
        }
    }
}