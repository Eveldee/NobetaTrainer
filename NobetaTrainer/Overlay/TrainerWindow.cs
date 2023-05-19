using ImGuiNET;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay
{
    private void ShowTrainerWindow()
    {
        ImGui.Begin("NobetaTrainer", ref OverlayState.ShowOverlay);

        ImGui.TextColored(TitleColor, $"Welcome to NobetaTrainer v{MyPluginInfo.PLUGIN_VERSION}");

        #if V1031
        ImGui.TextColored(ValueColor, "Using v1.0.3.1 compatibility fix");
        #endif

        // Window options
        if (ImGui.CollapsingHeader("Windows", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Inspector    ", ref OverlayState.ShowInspectWindow);
            ImGui.SameLine();
            ImGui.Checkbox("Teleportation", ref OverlayState.ShowTeleportationWindow);

            ImGui.Checkbox("Timers Config", ref OverlayState.ShowTimersConfigWindow);
            ImGui.SameLine();
            ImGui.Checkbox("Shortcut Editor", ref OverlayState.ShowShortcutEditorWindow);
        }

        // Character options
        if (ImGui.CollapsingHeader("Character", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.SeparatorText("General");
            ImGui.Checkbox("No Damage", ref CharacterPatches.NoDamageEnabled);
            HelpMarker("Ignore damages, disabling any effect like knockback");

            ImGui.Checkbox("Infinite HP", ref CharacterPatches.InfiniteHpEnabled);
            HelpMarker("Regen HP anytime it goes below max");

            ImGui.Checkbox("Infinite Mana", ref CharacterPatches.InfiniteManaEnabled);
            HelpMarker("Regen Mana anytime it goes below max");

            ImGui.Checkbox("Infinite Stamina", ref CharacterPatches.InfiniteStaminaEnabled);
            HelpMarker("Regen Stamina anytime it goes below max");

            ImGui.NewLine();
            if (ImGui.Checkbox("Nobeta Moveset", ref AppearancePatches.UseNobetaMoveset))
            {
                AppearancePatches.ToggleNobetaSkin();
            }
            HelpMarker("Enable Nobeta moveset that should only be usable in the second part of the game, also enable Nobeta skin as a side effect");

            ImGui.SeparatorText("Items");
            ImGui.DragInt("Souls", ref CharacterPatches.SoulsCount, 10, 0, 99_999);
            ImGui.SameLine();
            if (ImGui.Button("Set"))
            {
                CharacterPatches.SetSouls();
            }

            ImGui.NewLine();
            if (ImGui.Button("Give ##HP"))
            {
                ItemPatches.GiveHPItem();
            }
            ImGui.SameLine();
            ImGui.Combo("HP", ref ItemPatches.SelectedHPItemIndex, ItemUtils.HPItemNames, ItemUtils.HPItemNames.Length);

            if (ImGui.Button("Give ##MP"))
            {
                ItemPatches.GiveMPItem();
            }
            ImGui.SameLine();
            ImGui.Combo("MP", ref ItemPatches.SelectedMPItemIndex, ItemUtils.MPItemNames, ItemUtils.MPItemNames.Length);

            if (ImGui.Button("Give ##Buff"))
            {
                ItemPatches.GiveBuffItem();
            }
            ImGui.SameLine();
            ImGui.Combo("Buff", ref ItemPatches.SelectedBuffItemIndex, ItemUtils.BuffItemNames, ItemUtils.BuffItemNames.Length);

            if (ImGui.Button("Spawn##Other"))
            {
                ItemPatches.SpawnOther();
            }
            ImGui.SameLine();
            ImGui.Combo("Other", ref ItemPatches.SelectedOtherItemIndex, ItemUtils.OtherItemNames, ItemUtils.OtherItemNames.Length);

            ImGui.NewLine();
            if (ImGui.SliderInt("Item Slots", ref ItemPatches.ItemSlots, 4, 8))
            {
                ItemPatches.UpdateSlots();
            }


            ImGui.SeparatorText("Appearance");

            ImGui.Combo("Selected skin", ref AppearancePatches.SelectedSkinIndex, AppearancePatches.AvailableSkins,
                AppearancePatches.AvailableSkins.Length);
            if (ImGui.Button("Load Selected Skin"))
            {
                AppearancePatches.LoadSelectedSkin();
            }

            ImGui.NewLine();
            if (ImGui.Checkbox("Hide Bag", ref AppearancePatches.HideBagEnabled))
            {
                AppearancePatches.UpdateAppearance();
            }

            if (ImGui.Checkbox("Hide Staff", ref AppearancePatches.HideStaffEnabled))
            {
                AppearancePatches.UpdateAppearance();
            }

            if (ImGui.Checkbox("Hide Story Hat", ref AppearancePatches.HideHatEnabled))
            {
                AppearancePatches.UpdateAppearance();
            }

            ImGui.SeparatorText("Movements");

            if (ImGui.Checkbox("Enable NoClip", ref MovementPatches.NoClipEnabled))
            {
                MovementPatches.ToggleNoClip();
            }
            HelpMarker("Disable Nobeta collider, meaning that her hitbox is basically non-existent. Will allow to traverse walls but will also disable any trigger that works on her hitbox (map loading, fog, ...)");

            ImGui.SameLine();
            ImGui.Checkbox("Enable Glide", ref MovementPatches.GlideEnabled);
            HelpMarker("Allows moving in any direction. Velocity specifies the speed. Use jump key to go up and dodge key to go down. Speed is doubled when dashing (sprint)");
            ImGui.SliderFloat("Glide Velocity", ref MovementPatches.GlideVelocity, 0f, 20f);
        }

        // Magic options
        if (ImGui.CollapsingHeader("Magic"))
        {
            if (Singletons.GameSave is null)
            {
                ImGui.Text("No save loaded...");
            }
            else
            {
                if (ImGui.DragInt("Arcane Level", ref CharacterPatches.ArcaneMagicLevel, 0.1f, 1, 5))
                {
                    CharacterPatches.SetArcaneLevel();
                }

                if (ImGui.DragInt("Ice Level", ref CharacterPatches.IceMagicLevel, 0.1f, 1, 5))
                {
                    CharacterPatches.SetIceLevel();
                }

                if (ImGui.DragInt("Fire Level", ref CharacterPatches.FireMagicLevel, 0.1f, 1, 5))
                {
                    CharacterPatches.SetFireLevel();
                }

                if (ImGui.DragInt("Thunder Level", ref CharacterPatches.ThunderMagicLevel, 0.1f, 1, 5))
                {
                    CharacterPatches.SetThunderLevel();
                }

                if (ImGui.DragInt("Wind Level", ref CharacterPatches.WindMagicLevel, 0.1f, 1, 5))
                {
                    CharacterPatches.SetWindLevel();
                }

                if (ImGui.DragInt("Absorption Level", ref CharacterPatches.AbsorbMagicLevel, 0.1f, 1, 5))
                {
                    CharacterPatches.SetAbsorptionLevel();
                }
            }
        }

        // Combat options
        if (ImGui.CollapsingHeader("Combat"))
        {
            ImGui.SeparatorText("General");

            ImGui.Checkbox("One Tap", ref CharacterPatches.OneTapEnabled);
            HelpMarker("Kill all enemies in one hit, effectively deals just a stupid amount of damage");
        }

        // Other options
        if (ImGui.CollapsingHeader("Others"))
        {
            ImGui.SeparatorText("Environment");

            if (ImGui.Checkbox("Bright Mode", ref OtherPatches.BrightMode))
            {
                OtherPatches.UpdateBrightMode();
            }
            ImGui.SameLine();
            if (ImGui.DragFloat("##BrightModeIntensity", ref OtherPatches.BrightModeIntensity, 0.05f, 0f, 10f))
            {
                OtherPatches.UpdateBrightMode();
            }
            if (ImGui.ColorEdit3("Bright Mode Color", ref OtherPatches.BrightModeColor))
            {
                OtherPatches.UpdateBrightMode();
            }
            ImGui.NewLine();

            if (ImGui.Button("Remove Lava"))
            {
                OtherPatches.RemoveLava();
            }

            ImGui.SeparatorText("Save");

            if (Singletons.GameSave is null)
            {
                ImGui.Text("Please load a save first...");
            }
            else
            {
                if (ImGui.Checkbox("Show Teleport menu", ref OtherPatches.ForceShowTeleportMenu))
                {
                    OtherPatches.SetShowTeleportMenu();
                }
            }
        }

        ImGui.End();
    }
}