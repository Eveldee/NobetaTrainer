using System.Collections.Generic;
using NobetaTrainer.Trainer;
using UnityEngine;

namespace NobetaTrainer.Utils;

public static class UiHelpers
{
    public static void ForceCloseAllUi()
    {
        foreach (var ui in GameUis)
        {
            ui.SimpleHide();
        }
    }

    public static IEnumerable<GameCanvasBase> GameUis => Singletons.GameUIManager?.GetComponentsInChildren<GameCanvasBase>();

    public static void ToggleHudVisibility(bool visibility)
    {
        var magicBar = UnityUtils.FindGameObjectByNameForced("MagicBar");
        var playerStatsRoot = UnityUtils.FindGameObjectByNameForced("PlayerStatsRoot");

        magicBar.SetActive(visibility);
        playerStatsRoot.SetActive(visibility);
    }
}