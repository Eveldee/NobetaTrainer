using System;
using HarmonyLib;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Patches;

public static class CharacterPatches
{
    public static bool NoDamageEnabled;
    public static bool InfiniteHpEnabled;
    public static bool InfiniteManaEnabled;
    public static bool InfiniteStaminaEnabled;
    public static bool OneTapEnabled;

    public static int SoulsCount;

    public static int ArcaneMagicLevel;
    public static int IceMagicLevel;
    public static int FireMagicLevel;
    public static int ThunderMagicLevel;
    public static int WindMagicLevel;
    public static int AbsorbMagicLevel;

    public static void SetSouls()
    {
        if (Singletons.WizardGirl is not null)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                Game.GameSave.stats.currentMoney = SoulsCount;
                Game.UpdateMoney(SoulsCount);
            });
        }
    }

    public static void SetArcaneLevel()
    {
        SetStat(ArcaneMagicLevel, (stats, value) => stats.secretMagicLevel = value);
    }

    public static void SetIceLevel()
    {
        SetStat(IceMagicLevel, (stats, value) => stats.iceMagicLevel = value);
    }

    public static void SetFireLevel()
    {
        SetStat(FireMagicLevel, (stats, value) => stats.fireMagicLevel = value);
    }

    public static void SetThunderLevel()
    {
        SetStat(ThunderMagicLevel, (stats, value) => stats.thunderMagicLevel = value);
    }

    public static void SetWindLevel()
    {
        SetStat(WindMagicLevel, (stats, value) => stats.windMagicLevel = value);
    }

    public static void SetAbsorptionLevel()
    {
        SetStat(AbsorbMagicLevel, (stats, value) => stats.manaAbsorbLevel = value);
    }

    private static void SetStat<TValue>(TValue value, Action<PlayerStatsData, TValue> propertyModifier)
    {
        if (Singletons.GameSave is not { } gameSave)
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            propertyModifier(gameSave.stats, value);
        });
    }

    // No Damage
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Hit))]
    [HarmonyPrefix]
    private static bool WizardGirlManageHitPrefix(ref bool __result, AttackData Data, bool bIgnoreDodge = false)
    {
        // No Damage
        return !NoDamageEnabled;
    }

    // Infinite HP, MP and SP
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    [HarmonyPrefix]
    private static void WizardGirlManageUpdatePrefix()
    {
        var data = Singletons.WizardGirl.BaseData;

        // Infinite HP
        if (InfiniteHpEnabled)
        {
            // Increase mana if needed
            if (data.g_fHP < data.g_fHPMax)
            {
                data.g_fHP = data.g_fHPMax;
            }
        }

        // Infinite Mana
        if (InfiniteManaEnabled)
        {
            // Increase mana if needed
            if (data.g_fMP < data.g_fMPMax)
            {
                data.g_fMP = data.g_fMPMax;
            }
        }

        // Infinite Stamina
        if (InfiniteStaminaEnabled)
        {
            // Increase stamina if needed
            if (data.g_fSP < data.g_fSPMax)
            {
                data.g_fSP = data.g_fSPMax;
            }
        }
    }

    // One Tap
    [HarmonyPatch(typeof(NPCManage), nameof(NPCManage.Hit))]
    [HarmonyPrefix]
    private static void Hit(NPCManage __instance, AttackData Data)
    {
        // Kill NPC if 'One Tap' is activated
        if (OneTapEnabled)
        {
            Data.g_fStrength = float.MaxValue;
        }
    }

    // Update ItemSlots on load
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    [HarmonyPostfix]
    private static void WizardGirlManageInitPostfix(WizardGirlManage __instance)
    {
        var stats = __instance.GameSave.stats;

        ArcaneMagicLevel = stats.secretMagicLevel;
        IceMagicLevel = stats.iceMagicLevel;
        FireMagicLevel = stats.fireMagicLevel;
        ThunderMagicLevel = stats.thunderMagicLevel;
        WindMagicLevel = stats.windMagicLevel;
        AbsorbMagicLevel = stats.manaAbsorbLevel;
    }
}