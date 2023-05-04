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

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Dispose))]
    [HarmonyPrefix]
    static void WizardGirlManageDisposePrefix(WizardGirlManage __instance)
    {
        Plugin.Log.LogDebug($"{nameof(WizardGirlManage)} disposed");

        Instance = null;
        RuntimeData = null;
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
        var player = Instance;
        var data = player.BaseData;

        // Infinite HP
        if (Plugin.TrainerOverlay.InfiniteHpEnabled)
        {
            // Increase mana if needed
            if (data.g_fHP < data.g_fHPMax)
            {
                data.g_fHP = data.g_fHPMax;
            }
        }

        // Infinite Mana
        if (Plugin.TrainerOverlay.InfiniteManaEnabled)
        {
            // Increase mana if needed
            if (data.g_fMP < data.g_fMPMax)
            {
                data.g_fMP = data.g_fMPMax;
            }
        }

        // Infinite Stamina
        if (Plugin.TrainerOverlay.InfiniteStaminaEnabled)
        {
            // Increase stamina if needed
            if (data.g_fSP < data.g_fSPMax)
            {
                data.g_fSP = data.g_fSPMax;
            }
        }
    }
}