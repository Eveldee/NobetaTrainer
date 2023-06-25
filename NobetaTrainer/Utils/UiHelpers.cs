using System.Collections.Generic;

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
}