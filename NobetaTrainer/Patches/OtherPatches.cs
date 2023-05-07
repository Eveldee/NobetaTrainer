using System.Linq;
using HarmonyLib;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Patches;

public static class OtherPatches
{
    public static bool ForceShowTeleportMenu;

    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.StartGamePlay))]
    [HarmonyPostfix]
    private static void StartGamePlayPostfix(GameSave gameSave)
    {
        ForceShowTeleportMenu = Game.GameSave.basic.showTeleportMenu;
    }

    public static void RemoveLava()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            var gameObjects = Object.FindObjectsOfType<GameObject>();

            foreach (var gameObject in gameObjects)
            {
                // Visual Lava
                if (EnvironmentUtils.LavaTrapNamePrefix.Any(prefix => gameObject.name.StartsWith(prefix)))
                {
                    Object.Destroy(gameObject);
                }
            }
        });
    }

    public static void SetShowTeleportMenu()
    {
        if (Singletons.GameSave is not { } gameSave)
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            gameSave.basic.showTeleportMenu = ForceShowTeleportMenu;
        });
    }
}