using System;
using HarmonyLib;

namespace NobetaTrainer.Patches;

public static class ConfigPatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave), new Type[]{ })]
    [HarmonyPostfix]
    public static void WriteGameSavePostfix()
    {
        Plugin.SaveConfigs();
    }
}