using System.Threading.Tasks;
using Il2CppInterop.Runtime;
using ImGuiNET;
using NobetaTrainer.Timer;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Overlay;

public partial class NobetaTrainerOverlay
{
    private bool _showImGuiAboutWindow;
    private bool _showImGuiStyleEditorWindow;
    private bool _showImGuiDebugLogWindow;
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiUserGuideWindow;
    private bool _showImGuiStackToolWindow;

    public void Create()
    {
        DearImGuiInjection.DearImGuiInjection.Render += Render;
    }

    public void Destroy()
    {
        DearImGuiInjection.DearImGuiInjection.Render -= Render;
    }

    private void Render()
    {
        // Timers are always visible when activated, even if the overlay is hidden
        if (Timers.ShowTimers)
        {
            ShowTimersWindow();
        }

        // Velocity overlay is always visible when activated
        if (_showVelocityOverlay && Singletons.WizardGirl is { } wizardGirl)
        {
            ImGui.Begin("velocity", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground);

            ImGui.SetWindowSize(new(500, -1));

            PlotVelocity(wizardGirl.characterController.velocity, height: 200);

            ImGui.End();
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

        if (OverlayState.ShowSavesWindow)
        {
            ShowSavesWindow();
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