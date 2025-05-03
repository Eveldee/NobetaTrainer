using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DearImGuiInjection.Windows;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DearImGuiInjection.BepInEx;

internal static class LogInitier
{
    internal static void Init(ManualLogSource log)
    {
        Log.Init(new BepInExLog(log));
    }
}

[BepInPlugin(Metadata.GUID, Metadata.Name, Metadata.Version)]
internal class DearImGuiInjectionBasePluginIL2CPP : BasePlugin
{
    public override void Load()
    {
        LogInitier.Init(Log);

        Application.quitting += (Action) OnDestroy;

        var imGuiIniConfigDirectoryPath = Path.Combine(Paths.ConfigPath, "..", "..");

        var myPluginInfo = IL2CPPChainloader.Instance.Plugins[Metadata.GUID];
        var assetsFolder = Path.Combine(Path.GetDirectoryName(myPluginInfo.Location)!, "Assets");

        DearImGuiInjection.Init(imGuiIniConfigDirectoryPath, assetsFolder);
    }

    private void OnDestroy()
    {
        DearImGuiInjection.Dispose();
    }
}