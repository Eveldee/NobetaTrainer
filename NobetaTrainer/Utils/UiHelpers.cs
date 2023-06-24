using System.Collections.Generic;

namespace NobetaTrainer.Utils;

public static class UiHelpers
{
    public static void ForceCloseAllUi()
    {
        var uis = GameUis;

        foreach (var ui in uis)
        {
            ui.Hide(null);
        }
    }

    public static IEnumerable<GameCanvasBase> GameUis => Singletons.GameUIManager?.GetComponentsInChildren<GameCanvasBase>();
}