using HarmonyLib;

namespace NobetaTrainer.Patches;

public class NpcManagePatches
{
    [HarmonyPatch(typeof(NPCManage), nameof(NPCManage.Hit))]
    [HarmonyPrefix]
    static void Hit(NPCManage __instance, AttackData Data)
    {
        // Kill NPC if 'One Tap' is activated
        if (Plugin.TrainerOverlay.OneTapEnabled)
        {
            Data.g_fStrength = float.MaxValue;
        }
    }
}