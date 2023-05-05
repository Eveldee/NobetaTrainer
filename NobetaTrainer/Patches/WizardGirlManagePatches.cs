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

    #region SkinManager

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.LoadSkin))]
    [HarmonyPrefix]
    static void LoadSkinPrefix()
    {
        Plugin.Log.LogDebug("LoadSkin");
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.PreloadSkin))]
    [HarmonyPrefix]
    static void PreloadSkinPrefix()
    {
        Plugin.Log.LogDebug("PreloadSkin");
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.UpdateSkin))]
    [HarmonyPrefix]
    static void UpdateSkinPrefix()
    {
        Plugin.Log.LogDebug("UpdateSkin");
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.ReplaceActiveSkin))]
    [HarmonyPrefix]
    static void ReplaceActiveSkinPrefix()
    {
        Plugin.Log.LogDebug("ReplaceActiveSkin");
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.UseActiveSkin))]
    [HarmonyPrefix]
    static void UseActiveSkinPrefix()
    {
        Plugin.Log.LogDebug("UseActiveSkin");
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.UseStorySkin))]
    [HarmonyPrefix]
    static void UseStorySkinPrefix()
    {
        Plugin.Log.LogDebug("UseStorySkin");
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.GetSkinAssetKey))]
    [HarmonyPrefix]
    static void GetSkinAssetKeyPrefix()
    {
        Plugin.Log.LogDebug("GetSkinAssetKey");
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.ChangeSkin))]
    [HarmonyPrefix]
    static void A()
    {
        Plugin.Log.LogDebug("PlayerController.ChangeSkin");
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.UpdateSkin))]
    [HarmonyPrefix]
    static void B()
    {
        Plugin.Log.LogDebug("PlayerController.UpdateSkin");
    }

    [HarmonyPatch(typeof(PlayerController), nameof(GameCollection.UpdateSkin))]
    [HarmonyPrefix]
    static void C()
    {
        Plugin.Log.LogDebug("GameColletion.UpdateSkin");
    }

    [HarmonyPatch(typeof(UISkin.__c), nameof(UISkin.__c._UpdateActiveSkins_b__41_0))]
    [HarmonyPrefix]
    static void D()
    {
        Plugin.Log.LogDebug("UISkin.__c._UpdateActiveSkins_b__41_0");
    }

    #endregion
}