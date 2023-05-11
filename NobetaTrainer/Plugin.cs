using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Config;
using NobetaTrainer.Overlay;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("LittleWitchNobeta")]
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;

    public static TrainerOverlay TrainerOverlay;
    public static DirectoryInfo ConfigDirectory;
    public static ConfigFile ConfigFile;

    private static AutoConfigManager AutoConfigManager;

    public override void Load()
    {
        Log = base.Log;
        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} is loading...");

        Application.quitting += (Action) (() =>
        {
            TrainerOverlay.Close();
            Unload();
        });

        // Plugin startup logic
        ConfigFile = Config;
        ConfigDirectory = new DirectoryInfo(Path.GetDirectoryName(Config.ConfigFilePath)!);

        AutoConfigManager = new AutoConfigManager(Config);
        AutoConfigManager.LoadValuesToFields();

        // Fetch Nobeta process early to get game window handle
        NobetaProcessUtils.NobetaProcess = Process.GetProcessesByName("LittleWitchNobeta")[0];
        NobetaProcessUtils.GameWindowHandle = NobetaProcessUtils.NobetaProcess.MainWindowHandle;

        // Create and show overlay
        TrainerOverlay = new TrainerOverlay();
        Task.Run(TrainerOverlay.Run);

        // Apply patches
        ApplyPatches();

        // Add UnityMainThreadDispatcher
        AddComponent<UnityMainThreadDispatcher>();
        Singletons.ShortcutEditor = AddComponent<ShortcutEditor>();

        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} successfully loaded!");
    }

    public override bool Unload()
    {
        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} unloading...");

        AutoConfigManager.FetchValuesFromFields();
        Config.Save();

        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} successfully unloaded");

        return false;
    }

    public static void SaveConfigs()
    {
        Log.LogInfo("Saving configs...");

        // TODO Save shortcuts

        // Save BepInEx config
        AutoConfigManager.FetchValuesFromFields();
        ConfigFile.Save();

        Log.LogInfo("Configs saved");
    }

    public static void ApplyPatches()
    {
        Harmony.CreateAndPatchAll(typeof(Singletons));
        Harmony.CreateAndPatchAll(typeof(CharacterPatches));
        Harmony.CreateAndPatchAll(typeof(AppearancePatches));
        Harmony.CreateAndPatchAll(typeof(MovementPatches));
        Harmony.CreateAndPatchAll(typeof(OtherPatches));
        Harmony.CreateAndPatchAll(typeof(ItemPatches));
        Harmony.CreateAndPatchAll(typeof(ShortcutEditor));
        Harmony.CreateAndPatchAll(typeof(ConfigPatches));
    }
}