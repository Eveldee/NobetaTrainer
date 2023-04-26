using HarmonyLib;
using UnityEngine;

namespace NobetaTrainer;

public class WizardGirlManagePatches
{
    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    [HarmonyPrefix]
    static void UpdatePrefix()
    {

    }
}