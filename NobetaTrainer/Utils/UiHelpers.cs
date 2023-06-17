using NobetaTrainer.Trainer;

namespace NobetaTrainer.Utils;

public static class UiHelpers
{
    public static void ForceCloseAllUi()
    {
        if (Singletons.GameInstance.ui.stackingManager.canvasStack is { } uiStack)
        {
            while (uiStack.Count > 0)
            {
                var ui = uiStack.Pop();
                ui.ForceClose();
            }
        }
    }
}