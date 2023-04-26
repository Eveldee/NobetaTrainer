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
            var sceneManager = Object.FindObjectOfType<SceneManager>();
            var player = sceneManager.GetPlayerManager();
            var data = player.BaseData;

            // Increase mana if needed
            if (data.g_fMP < data.g_fMPMax)
            {
                data.g_fMP = data.g_fMPMax;

                Plugin.Log.LogDebug($"Set MP to Max ({data.g_fMPMax}) because 'infinite mana' is activated");

                return false;
            }
        }

        return true;
    }
}