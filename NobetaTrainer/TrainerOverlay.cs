using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using Il2CppInterop.Runtime;
using ImGuiNET;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer
{
    public class TrainerOverlay : Overlay
    {
        private bool _showImGuiAboutWindow;
        private bool _showImGuiStyleEditorWindow;
        private bool _showImGuiDebugLogWindow;
        private bool _showImGuiDemoWindow;
        private bool _showImGuiMetricsWindow;
        private bool _showImGuiUserGuideWindow;
        private bool _showImGuiStackToolWindow;

        private readonly Vector4 _valueColor = new(252 / 255f, 161 / 255f, 3 / 255f, 1f);

        private bool _infiniteManaEnabled;
        public bool InfiniteManaEnabled => _infiniteManaEnabled;

        private bool _noDamageEnabled;
        public bool NoDamageEnabled => _noDamageEnabled;

        private bool _infiniteStaminaEnabled;
        public bool InfiniteStaminaEnabled => _infiniteStaminaEnabled;

        private bool _isToolVisible = true;
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

            ShowTrainerWindow();
            ShowInspectWindow();
        }

        private void ShowTrainerWindow()
        {
            ImGui.Begin("NobetaTrainer", ref _isToolVisible);

            ImGui.Text($"Welcome to NobetaTrainer v{_assemblyVersion}");

            // Character options
            if (ImGui.CollapsingHeader("Character", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.SeparatorText("General");
                ImGui.Checkbox("No Damage", ref _noDamageEnabled);
                ImGui.Checkbox("Infinite Mana", ref _infiniteManaEnabled);
                ImGui.Checkbox("Infinite Stamina", ref _infiniteStaminaEnabled);

                ImGui.SeparatorText("Actions");
            }

            // Magic options
            if (ImGui.CollapsingHeader("Magic"))
            {

            }

            ImGui.End();
        }

        private void ShowInspectWindow()
        {
            void ShowValue(string title, object value, string format = null, string help = null)
            {
                ImGui.Text(title);
                ImGui.SameLine();

                if (format is null)
                {
                    ImGui.TextColored(_valueColor, string.Format(CultureInfo.InvariantCulture, "{0}", value));
                }
                else
                {
                    ImGui.TextColored(_valueColor, string.Format(CultureInfo.InvariantCulture, $"{{0:{format}}}", value));
                }

                if (help is not null)
                {
                    ImGui.SameLine();
                    HelpMarker(help);
                }
            }

            void ToggleButton(string title, ref bool valueToToggle)
            {
                if (ImGui.Button(title))
                {
                    valueToToggle = !valueToToggle;
                }
            }

            ImGui.Begin("NobetaTrainerInspector");

            if (ImGui.CollapsingHeader("ImGui"))
            {
                ImGui.SeparatorText("ImGui Windows");

                ImGui.Checkbox("About", ref _showImGuiAboutWindow);
                ImGui.SameLine();
                ImGui.Checkbox("Debug Logs", ref _showImGuiDebugLogWindow);
                ImGui.SameLine();
                ImGui.Checkbox("Demo", ref _showImGuiDemoWindow);
                ImGui.SameLine();
                ImGui.Checkbox("Metrics", ref _showImGuiMetricsWindow);

                ImGui.Checkbox("Style Editor", ref _showImGuiStyleEditorWindow);
                ImGui.SameLine();
                ImGui.Checkbox("Stack Tool", ref _showImGuiStackToolWindow);
                ImGui.SameLine();
                ImGui.Checkbox("User Guide", ref _showImGuiUserGuideWindow);

                ImGui.SeparatorText("Style");
                ImGui.ShowStyleSelector("Pick a style");
            }

            if (ImGui.CollapsingHeader("Unity Engine"))
            {
                ImGui.SeparatorText("Framerate");
                ShowValue("Target Framerate:", Application.targetFrameRate);
                ShowValue("Vsync enabled:", QualitySettings.vSyncCount.ToBool());
                ShowValue("Frame Count:", Time.frameCount);
                ShowValue("Realtime since startup:", TimeSpan.FromSeconds(Time.realtimeSinceStartup).ToString(FormatUtils.TimeSpanMillisFormat));
                ShowValue("Current Framerate:", 1f / Time.smoothDeltaTime, "F0");
                ShowValue("Mean Framerate:", Time.frameCount / Time.time, "F0");

                ImGui.SeparatorText("DeltaTime");
                ShowValue("DeltaTime:", Time.deltaTime);
                ShowValue("Fixed DeltaTime:", Time.fixedDeltaTime);
                ShowValue("Maximum DeltaTime:", Time.maximumDeltaTime);
            }

            if (ImGui.CollapsingHeader("NobetaRuntimeData", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (WizardGirlManagePatches.RuntimeData is not { } runtimeData)
                {
                    ImGui.TextWrapped("No runtime data available, load a character first...");
                }
                else
                {
                    ImGui.SeparatorText("Constants");
                    ShowValue("Absorb CD Time Max:", NobetaRuntimeData.ABSORB_CD_TIME_MAX, help: "Delay between absorb status");
                    ShowValue("Absorb Status Time Max:", NobetaRuntimeData.ABSORB_STATUS_TIME_MAX, help: "Duration of absorption");
                    ShowValue("Absorb Time Max:", NobetaRuntimeData.ABSORB_TIME_MAX, help: "Duration of absorb time status (time in which getting hit triggers an absorption");
                    ShowValue("Repulse Time Max:", NobetaRuntimeData.REPULSE_TIME_MAX);
                    ShowValue("Full Timer Limit:", NobetaRuntimeData.FULL_TIMER_LIMIT);

                    ImGui.SeparatorText("Absorb");
                    ShowValue("Absorb CD Timer:", runtimeData.AbsorbCDTimer);
                    ShowValue("Absorb Status Timer:", runtimeData.AbsorbStatusTimer);
                    ShowValue("Absorb Timer:", runtimeData.AbsorbTimer);

                    ImGui.SeparatorText("Movements");
                    ShowValue("Jump Direction:", runtimeData.JumpDirection.Format());
                    ShowValue("Move Direction:", runtimeData.moveDirection.Format());
                    ShowValue("Previous Position:", runtimeData.previousPosition.Format());
                }
            }

            ImGui.End();
        }

        private static void HelpMarker(string description)
        {
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
}