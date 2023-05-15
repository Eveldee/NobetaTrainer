using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Humanizer;
using Il2CppInterop.Runtime;
using ImGuiNET;
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

    private readonly string _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    protected override Task PostInitialized()
    {
        VSync = true;

        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());

        return Task.CompletedTask;
    }

    protected override void Render()
    {
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
        if (OverlayState.ShowShortcutEditorWindow)
        {
            ShowShortcutEditorWindow();
        }

        if (OverlayState.ShowTeleportationWindow)
        {
            ShowTeleportationWindow();
        }
    }

    private static void ShowValue(string title, object value, string format = null, string help = null)
    {
        ImGui.Text(title);
        ImGui.SameLine();

        if (format is null)
        {
            ImGui.TextColored(ValueColor, string.Format(CultureInfo.InvariantCulture, "{0}", value));
        }
        else
        {
            ImGui.TextColored(ValueColor, string.Format(CultureInfo.InvariantCulture, $"{{0:{format}}}", value));
        }

        if (help is not null)
        {
            HelpMarker(help);
        }
    }

    private static void ShowValueExpression(object value, string format = null, string help = null, [CallerArgumentExpression(nameof(value))] string valueExpression = default)
    {
        // TODO Replace by a compiled Regex
        // Remove .Get .Is and g_[b|f]
        valueExpression = valueExpression!.Replace(".Is", ".");
        valueExpression = valueExpression!.Replace(".GetIs", ".");
        valueExpression = valueExpression!.Replace(".Get", ".");
        valueExpression = valueExpression!.Replace(".g_f", ".");
        valueExpression = valueExpression!.Replace(".g_b", ".");
        valueExpression = valueExpression!.Replace(".g_", ".");
        valueExpression = valueExpression!.Replace("Null", "Arcane");

        if (valueExpression!.EndsWith("Format()"))
        {
            valueExpression = valueExpression[..valueExpression.LastIndexOf('.')];
            ShowValue($"{valueExpression![(valueExpression.LastIndexOf('.')+1)..].Humanize(LetterCasing.Title)}:", value, format, help);
        }
        else
        {
            ShowValue($"{valueExpression![(valueExpression.LastIndexOf('.')+1)..].Humanize(LetterCasing.Title)}:", value, format, help);
        }
    }

    private static void HelpMarker(string description, bool sameLine = true)
    {
        if (sameLine)
        {
            ImGui.SameLine();
        }

        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
        {
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    private bool ButtonColored(Vector4 color, string label, float gradientStep = 0.1f)
    {
        var gradient = color.IntensityGradient(gradientStep, 3);

        ImGui.PushStyleColor(ImGuiCol.Button, gradient[0]);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, gradient[1]);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, gradient[2]);

        var result = ImGui.Button(label);

        ImGui.PopStyleColor(3);

        return result;
    }
}