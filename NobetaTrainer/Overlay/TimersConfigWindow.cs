using ImGuiNET;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Patches;
using UnityEngine;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay
{
    private void ShowTimersConfigWindow()
    {
        ImGui.Begin("Timers Config", ref OverlayState.ShowTimersConfigWindow);

        ImGui.TextColored(InfoColor, "Timers");

        if (Singletons.Timers is null)
        {
            ImGui.Text("Waiting for timers to be loaded...");
        }
        else
        {
            ImGui.SeparatorText("General");

            ImGui.Checkbox("Show Timers", ref Timers.ShowTimers);
            ImGui.Checkbox("Pause Timers", ref Timers.PauseTimers);
            HelpMarker("This will pause timers on game pause (when opening the menu). Note that Real Time is unaffected as it shows time since game start");

            ImGui.SeparatorText("Style");
            ImGui.SliderFloat("X", ref _timersWindowPosition.X, _borderSize, Screen.width - _timersWindowSize.X - _borderSize);
            ImGui.SliderFloat("Y", ref _timersWindowPosition.Y, _borderSize, Screen.height - _timersWindowSize.Y - _borderSize);

            ImGui.DragFloat("Border", ref _borderSize, 1f, 0f, float.MaxValue);
            ImGui.DragFloat("Rounding", ref _borderRounding, 1f, 0f, float.MaxValue);

            ImGui.ColorEdit4("Text", ref _timersTextColor);
            ImGui.ColorEdit4("Background", ref _timersBackgroundColor);
            ImGui.ColorEdit4("Border", ref _timersBorderColor);

            ImGui.SeparatorText("Timers");

            ImGui.Checkbox("Real Time", ref Timers.ShowRealTime);
            ImGui.Checkbox("Last Load", ref Timers.ShowLastLoad);
            ImGui.Checkbox("Last Save", ref Timers.ShowLastSave);
            ImGui.Checkbox("Last Teleport", ref Timers.ShowLastTeleport);
        }

        ImGui.End();
    }
}