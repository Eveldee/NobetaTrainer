using System;
using HarmonyLib;

namespace NobetaTrainer.Trainer;

public static class ConfigPatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave), new Type[]{ })]
    [HarmonyPostfix]
    public static void WriteGameSavePostfix()
    {
        Plugin.Log.LogDebug("Triggered Config save on Game save");
        Plugin.SaveConfigs();
    }
}