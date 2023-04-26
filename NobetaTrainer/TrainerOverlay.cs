using System.Reflection;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace NobetaTrainer
{
    public class TrainerOverlay : Overlay
    {
        private const bool ShowDemoWindow = true;
        private const bool ShowMetricsWindow = false;

        private bool _isInfiniteManaEnabled;
        public bool IsInfiniteManaEnabled => _isInfiniteManaEnabled;

        private bool _noDamageEnabled;
        public bool NoDamageEnabled => _noDamageEnabled;

        private bool _isToolVisible = true;

        protected override Task PostInitialized()
        {
            VSync = true;

            return Task.CompletedTask;
        }

        protected override void Render()
        {
            if (ShowDemoWindow)
            {
                ImGui.ShowDemoWindow();
            }

            if (ShowMetricsWindow)
            {
                ImGui.ShowMetricsWindow();
            }

            ShowTrainerWindow();
        }

        private void ShowTrainerWindow()
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            ImGui.Begin("NobetaTrainer", ref _isToolVisible);

            ImGui.Text($"Welcome to NobetaTrainer v{assemblyVersion}");

            // Character options
            if (ImGui.CollapsingHeader("Character", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.SeparatorText("General");
                ImGui.Checkbox("Infinite Mana", ref _isInfiniteManaEnabled);
                ImGui.Checkbox("No Damage", ref _noDamageEnabled);

                ImGui.SeparatorText("Actions");
            }

            // Magic options
            if (ImGui.CollapsingHeader("Magic"))
            {

            }

            ImGui.End();
        }
    }
}