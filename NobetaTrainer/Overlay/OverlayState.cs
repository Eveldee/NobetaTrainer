using NobetaTrainer.Config;

namespace NobetaTrainer.Overlay;

[Section("Overlay")]
public static class OverlayState
{
    [Bind]
    public static bool ShowOverlay = true;
    [Bind]
    public static bool ShowInspectWindow;
    [Bind]
    public static bool ShowTeleportationWindow;

    public static bool ShowShortcutEditorWindow;
}