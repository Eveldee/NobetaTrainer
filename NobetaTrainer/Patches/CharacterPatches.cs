using System;
using HarmonyLib;
using NobetaTrainer.Config;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Patches;

[Section("Character.General")]
public static class CharacterPatches
{
    [Bind]
    public static bool NoDamageEnabled;
    [Bind]
    public static bool InfiniteHpEnabled;
    [Bind]
    public static bool InfiniteManaEnabled;
    [Bind]
    public static bool InfiniteStaminaEnabled;
    [Bind]
    public static bool OneTapEnabled;

    public static int SoulsCount;

    public static int ArcaneMagicLevel;
    public static int IceMagicLevel;
    public static int FireMagicLevel;
    public static int ThunderMagicLevel;
    public static int WindMagicLevel;
    public static int AbsorbMagicLevel;

    public static int HealthLevel;
    public static int ManaLevel;
    public static int StaminaLevel;
    public static int StrengthLevel;
    public static int IntelligenceLevel;
    public static int HasteLevel;

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

    // Magic level set
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

    // Abilities level set
    public static void SetHealthLevel()
    {
        SetStat(HealthLevel, (stats, value) => stats.healthyLevel = value, wizardGirl => wizardGirl.HPLevelUp());
    }
    public static void SetManaLevel()
    {
        SetStat(ManaLevel, (stats, value) => stats.manaLevel = value, wizardGirl => wizardGirl.MPLevelUp());
    }
    public static void SetStaminaLevel()
    {
        SetStat(StaminaLevel, (stats, value) => stats.staminaLevel = value, wizardGirl => wizardGirl.SPLevelUp());
    }
    public static void SetStrengthLevel()
    {
        SetStat(StrengthLevel, (stats, value) => stats.strengthLevel = value, wizardGirl => wizardGirl.OtherLevelUp());
    }
    public static void SetIntelligenceLevel()
    {
        SetStat(IntelligenceLevel, (stats, value) => stats.intelligenceLevel = value, wizardGirl => wizardGirl.OtherLevelUp());
    }
    public static void SetHasteLevel()
    {
        SetStat(HasteLevel, (stats, value) => stats.dexterityLevel = value, wizardGirl => wizardGirl.OtherLevelUp());
    }

    private static void SetStat<TValue>(TValue value, Action<PlayerStatsData, TValue> propertyModifier, Action<WizardGirlManage> afterUpdate = null)
    {
        if (Singletons.GameSave is not { } gameSave)
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            propertyModifier(gameSave.stats, value);

            if (Singletons.WizardGirl is { } wizardGirl)
            {
                afterUpdate?.Invoke(wizardGirl);
            }
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

        HealthLevel = stats.healthyLevel;
        ManaLevel = stats.manaLevel;
        StaminaLevel = stats.staminaLevel;
        StrengthLevel = stats.strengthLevel;
        IntelligenceLevel = stats.intelligenceLevel;
        HasteLevel = stats.dexterityLevel;
    }
}