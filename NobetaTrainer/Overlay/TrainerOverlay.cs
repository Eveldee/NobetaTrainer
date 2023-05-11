using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Humanizer;
using Il2CppInterop.Runtime;
using ImGuiNET;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay : ClickableTransparentOverlay.Overlay
{
    private static readonly Vector4 ValueColor = new(252 / 255f, 161 / 255f, 3 / 255f, 1f);
    private static readonly Vector4 InfoColor = new(3 / 255f, 148 / 255f, 252 / 255f, 1f);
    private static readonly Vector4 WarningColor = new(252 / 255f, 211 / 255f, 3 / 255f, 1f);
    private static readonly Vector4 TitleColor = new(173 / 255f, 3 / 255f, 252 / 255f, 1f);

    private bool _showImGuiAboutWindow;
    private bool _showImGuiStyleEditorWindow;
    private bool _showImGuiDebugLogWindow;
    private bool _showImGuiDemoWindow;
    private bool _showImGuiMetricsWindow;
    private bool _showImGuiUserGuideWindow;
    private bool _showImGuiStackToolWindow;
    public bool _showShortcutEditorWindow;
    private bool _showTeleportationWindow;

    private bool _showTrainerWindow = true;
    private bool _showInspectWindow = true;

    private readonly string _assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

    protected override Task PostInitialized()
    {
        VSync = true;

        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());

        return Task.CompletedTask;
    }

    protected override void Render()
    {
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

        if (_showTrainerWindow)
        {
            ShowTrainerWindow();
        }
        if (_showInspectWindow)
        {
           ShowInspectWindow();
        }
        if (_showShortcutEditorWindow)
        {
            ShowShortcutEditorWindow();
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
}