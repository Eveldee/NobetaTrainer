using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace NobetaTrainer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("LittleWitchNobeta")]
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;

    public static TrainerOverlay TrainerOverlay;

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        // Create and show overlay
        TrainerOverlay = new TrainerOverlay();
        Task.Run(TrainerOverlay.Run);

        // Apply patches
        Harmony.CreateAndPatchAll(typeof(Plugin));
        Harmony.CreateAndPatchAll(typeof(WizardGirlManagePatches));
        Harmony.CreateAndPatchAll(typeof(GamePatches));
    }

    [HarmonyPatch(typeof(UIPauseMenu), nameof(UIPauseMenu.Appear))]
    [HarmonyPrefix]
    static void PauseMenuOpenPrefix()
    {
        Log.LogInfo("Pause menu opened");
    }
}