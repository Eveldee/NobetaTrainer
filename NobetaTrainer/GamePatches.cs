using HarmonyLib;
using UnityEngine;

namespace NobetaTrainer;

public class GamePatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.UpdatePlayerMP))]
    [HarmonyPrefix]
    static bool UpdatePlayerMpPrefix(bool isCured)
    {
        if (Plugin.TrainerOverlay.IsInfiniteManaEnabled)
        {
            var player = WizardGirlManagePatches.Instance;
            var data = player.BaseData;

            // Increase mana if needed
            if (data.g_fMP < data.g_fMPMax)
            {
                data.g_fMP = data.g_fMPMax;

                return false;
            }
        }

        return true;
    }
}