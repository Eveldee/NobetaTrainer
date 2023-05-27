using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Humanizer;
using Il2CppInterop.Runtime;
using ImGuiNET;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Utils;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay : ClickableTransparentOverlay.Overlay
{
    private bool _showImGuiAboutWindow;
    private bool _showImGuiStyleEditorWindow;
    private bool _showImGuiDebugLogWindow;
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiUserGuideWindow;
    private bool _showImGuiStackToolWindow;

    protected override Task PostInitialized()
    {
        VSync = true;

        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
        NobetaProcessUtils.OverlayWindowHandle = NobetaProcessUtils.FindWindow(null, "Overlay");
        NobetaProcessUtils.HideOverlayFromTaskbar();

        return Task.CompletedTask;
    }

    protected override void Render()
    {
        // Timers are always visible when activated, even if overlay is hidden
        if (Timers.ShowTimers)
        {
            ShowTimersWindow();
        }

        if (!OverlayState.ShowOverlay)
        {
            return;
        }

        if (_showImGuiAboutWindow)
        {
            ImGui.ShowAboutWindow();
        }
        if (_showImGuiDebugLogWindow)
        {
            ImGui.ShowDebugLogWindow();
        }
        if (_showImGuiDemoWindow)
        {
            ImGui.ShowDemoWindow();
        }
        if (_showImGuiMetricsWindow)
        {
            ImGui.ShowMetricsWindow();
        }
        if (_showImGuiStyleEditorWindow)
        {
            ImGui.ShowStyleEditor();
        }
        if (_showImGuiStackToolWindow)
        {
            ImGui.ShowStackToolWindow();
        }
        if (_showImGuiUserGuideWindow)
        {
            ImGui.ShowUserGuide();
        }

        if (OverlayState.ShowOverlay)
        {
            ShowTrainerWindow();
        }
        if (OverlayState.ShowInspectWindow)
        {
           ShowInspectWindow();
        }
        if (OverlayState.ShowTeleportationWindow)
        {
            ShowTeleportationWindow();
        }
        if (OverlayState.ShowShortcutEditorWindow)
        {
            ShowShortcutEditorWindow();
        }
        if (OverlayState.ShowTimersConfigWindow)
        {
            ShowTimersConfigWindow();
        }
    }
}