using System;
using System.Diagnostics;
using ImGuiNET;
using NobetaTrainer.Config;
using NobetaTrainer.Overlay;
using NobetaTrainer.Utils;
using UnityEngine;
using ImGui = UnityEngine.GUILayout;

namespace NobetaTrainer.Behaviours;

[Section("Timers")]
public class Timers : MonoBehaviour
{
    [Bind]
    public static bool ShowTimers;
    [Bind]
    public static bool ShowRealTime;
    [Bind]
    public static bool ShowLastLoad;
    [Bind]
    public static bool ShowLastSave;
    [Bind]
    public static bool ShowLastTeleport;
    [Bind]
    public static bool PauseTimers;

    public TimeSpan RealTime => _realTimeTimer.Elapsed;
    public TimeSpan LastLoad => _lastLoadTimer.Elapsed;
    public TimeSpan LastSave => _lastSaveTimer.Elapsed;
    public TimeSpan LastTeleport => _lastTeleportTimer.Elapsed;

    private Stopwatch _realTimeTimer;
    private Stopwatch _lastLoadTimer;
    private Stopwatch _lastSaveTimer;
    private Stopwatch _lastTeleportTimer;

    private void Awake()
    {
        Plugin.Log.LogDebug("Timers initialized");

        _realTimeTimer = new Stopwatch();
        _realTimeTimer.Start();
        _lastLoadTimer = new Stopwatch();
        _lastSaveTimer = new Stopwatch();
        _lastTeleportTimer = new Stopwatch();
    }

    public void Pause()
    {
        if (PauseTimers)
        {
            _lastLoadTimer.Stop();
            _lastSaveTimer.Stop();
            _lastTeleportTimer.Stop();
        }
    }

    public void Resume()
    {
        _lastLoadTimer.Start();
        _lastSaveTimer.Start();
        _lastTeleportTimer.Start();
    }

    public void ResetLoadTimer()
    {
        _lastLoadTimer.Reset();
        _lastSaveTimer.Reset();
        _lastTeleportTimer.Reset();
    }

    public void ResetSaveTimer()
    {
        _lastSaveTimer.Reset();
        _lastTeleportTimer.Reset();
    }

    public void ResetTeleportTimer()
    {
        _lastTeleportTimer.Reset();
    }
}