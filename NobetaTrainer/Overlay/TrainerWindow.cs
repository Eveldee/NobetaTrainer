﻿using System.Linq;
using System.Numerics;
using Humanizer;
using ImGuiNET;
using NobetaTrainer.Colliders;
using NobetaTrainer.Trainer;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Overlay;

public partial class NobetaTrainerOverlay
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
            ImGui.SameLine();
            ImGui.Checkbox("Saves", ref OverlayState.ShowSavesWindow);

            ImGui.Checkbox("Timers Config", ref OverlayState.ShowTimersConfigWindow);
            ImGui.SameLine();
            ImGui.Checkbox("Shortcut Editor", ref OverlayState.ShowShortcutEditorWindow);
        }

        // Character options
        if (ImGui.CollapsingHeader("Character", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Combat options
            ImGui.SeparatorText("Combat");

            ImGui.Checkbox("Vanilla Mode", ref CharacterPatches.VanillaMode);
            HelpMarker("Disable all 'cheat' features like Infinite HP, No Damage, One Tap, ...");

            ImGui.Separator();

            WithDisabled(CharacterPatches.VanillaMode, () =>
            {
                ImGui.Checkbox("Infinite HP", ref CharacterPatches.InfiniteHpEnabled);
                HelpMarker("Regen HP anytime it goes below max");

                ImGui.Checkbox("Infinite Mana", ref CharacterPatches.InfiniteManaEnabled);
                HelpMarker("Regen Mana anytime it goes below max");

                ImGui.Checkbox("Infinite Stamina", ref CharacterPatches.InfiniteStaminaEnabled);
                HelpMarker("Regen Stamina anytime it goes below max");

                ImGui.Separator();

                ImGui.Checkbox("No Damage", ref CharacterPatches.NoDamageEnabled);
                HelpMarker("Ignore damages, disabling any effect like knockback");

                ImGui.Checkbox("One Tap", ref CharacterPatches.OneTapEnabled);
                HelpMarker("Kill all enemies in one hit, effectively deals just a stupid amount of damage");

                ImGui.Checkbox("On Hit KO (OHKO)", ref CharacterPatches.OneHitKOEnabled);
                HelpMarker("Nobeta instantly dies upon taking any damage");
            });

            ImGui.Separator();

            if (ImGui.Checkbox("Force Nobeta Moveset", ref AppearancePatches.ForceNobetaMoveset))
            {
                AppearancePatches.ToggleNobetaMoveset();
            }
            HelpMarker("Enable Nobeta moveset that should only be usable in the second part of the game, also enable Nobeta skin as a side effect. Return to statue to disable.");

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

            ImGui.NewLine();
            if (ImGui.Checkbox("Invisible Nobeta", ref AppearancePatches.InvisibleEnabled))
            {
                AppearancePatches.UpdateAppearance();
            }

            ImGui.SeparatorText("Movements");

            ImGui.Checkbox("Enable NoClip", ref MovementPatches.NoClipEnabled);
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

        // Stats options
        // Magic options
        if (ImGui.CollapsingHeader("Ability Stats"))
        {
            if (Singletons.GameSave is null)
            {
                ImGui.Text("No save loaded...");
            }
            else
            {
                if (ImGui.DragInt("Health Level", ref CharacterPatches.HealthLevel, 0.1f, 1, 100))
                {
                    CharacterPatches.SetHealthLevel();
                }

                if (ImGui.DragInt("Mana Level", ref CharacterPatches.ManaLevel, 0.1f, 1, 100))
                {
                    CharacterPatches.SetManaLevel();
                }

                if (ImGui.DragInt("Stamina Level", ref CharacterPatches.StaminaLevel, 0.1f, 1, 100))
                {
                    CharacterPatches.SetStaminaLevel();
                }

                if (ImGui.DragInt("Strength Level", ref CharacterPatches.StrengthLevel, 0.1f, 1, 100))
                {
                    CharacterPatches.SetStrengthLevel();
                }

                if (ImGui.DragInt("Intelligence Level", ref CharacterPatches.IntelligenceLevel, 0.1f, 1, 100))
                {
                    CharacterPatches.SetIntelligenceLevel();
                }

                if (ImGui.DragInt("Haste Level", ref CharacterPatches.HasteLevel, 0.1f, 1, 50))
                {
                    CharacterPatches.SetHasteLevel();
                }
            }
        }

        // Colliders options
        if (ImGui.CollapsingHeader("Colliders"))
        {
            ImGui.SeparatorText("General");

            if (ImGui.Checkbox("Show Colliders", ref CollidersRenderPatches.ShowColliders))
            {
                CollidersRenderPatches.ToggleShowColliders();
            }
            ImGui.Checkbox("Enable Other Colliders loading", ref CollidersRenderPatches.EnableOtherColliders);
            HelpMarker("This can affect performances negatively!!! Takes effect after reloading the scene. Note that this won't affect scene event colliders as they don't generate any lag");

            ImGui.SeparatorText($"Colliding Scene Events ({CollidersRenderPatches.CollidingSceneEvents?.Sum(group => group.Count()) ?? 0}) [Point approximation]");
            Child("Colliding Scene Events#Child", new Vector2(0, 200), true, ImGuiWindowFlags.AlwaysVerticalScrollbar, () =>
            {
                if (CollidersRenderPatches.CollidingSceneEvents is { } collidingSceneEvents)
                {
                    foreach (var collidingSceneEventGroup in collidingSceneEvents)
                    {
                        TreeNodeEx($"{collidingSceneEventGroup.Key}##SceneEvent{collidingSceneEventGroup.Key}", ImGuiTreeNodeFlags.DefaultOpen, () =>
                        {
                            foreach (var sceneEvent in collidingSceneEventGroup)
                            {
                                ImGui.TextColored(ValueColor, $"{sceneEvent.name}");
                                if (sceneEvent.g_bOpenEvent)
                                {
                                    ImGui.SameLine();
                                    ImGui.TextColored(WarningColor, "(Open)");
                                }

                                if (sceneEvent.g_bReleaseEvent)
                                {
                                    ImGui.SameLine();
                                    ImGui.TextColored(WarningColor, "(Release)");
                                }

                                if (!sceneEvent.isActiveAndEnabled)
                                {
                                    ImGui.SameLine();
                                    ImGui.TextColored(ErrorColor, "(Disabled)");
                                }
                            }
                        });
                    }
                }
            });

            ImGui.SeparatorText($"Colliding Active Colliders ({CollidersRenderPatches.CollidingColliders?.Sum(group => group.Count()) ?? 0}) [Exact]");
            Child("Colliding Colliders#Child", new Vector2(0, 200), true, ImGuiWindowFlags.AlwaysVerticalScrollbar, () =>
            {
                if (CollidersRenderPatches.CollidingColliders is {  } collidingColliders)
                {
                    foreach (var grouping in collidingColliders)
                    {
                        TreeNodeEx($"{grouping.Key}##Collider{grouping.Key}", ImGuiTreeNodeFlags.DefaultOpen, () =>
                        {
                            foreach (var collider in grouping)
                            {
                                ImGui.TextColored(ValueColor, $"{collider.name}");
                                if (collider.isTrigger)
                                {
                                    ImGui.SameLine();
                                    ImGui.TextColored(WarningColor, "(Trigger)");
                                }
                            }
                        });
                    }
                }
            });

            ImGui.SeparatorText("Styles");

            foreach (var (colliderType, rendererConfig) in Singletons.ColliderRendererManager.RendererConfigs)
            {
                TreeNode($"{colliderType.Humanize()}##{colliderType}", () =>
                {
                    if (colliderType == ColliderType.Other && !CollidersRenderPatches.EnableOtherColliders)
                    {
                        ImGui.BeginDisabled();
                    }

                    if (ImGui.Checkbox($"Enable##{colliderType}", ref rendererConfig.Enable))
                    {
                        CollidersRenderPatches.UpdateDrawLines(colliderType);
                    }

                    ImGui.SameLine();
                    if (ImGui.Checkbox($"Draw Lines##{colliderType}", ref rendererConfig.DrawLines))
                    {
                        CollidersRenderPatches.UpdateDrawLines(colliderType);
                    }

                    ImGui.SameLine();
                    if (ImGui.Checkbox($"Draw Surfaces##{colliderType}", ref rendererConfig.DrawSurfaces))
                    {
                        CollidersRenderPatches.UpdateDrawLines(colliderType);
                    }

                    if (ImGui.InputFloat($"Line width##{colliderType}", ref rendererConfig.LineWidth, 0.01f, 0.1f,
                            "%.2f"))
                    {
                        CollidersRenderPatches.UpdateDrawLines(colliderType);
                    }

                    if (ImGui.ColorEdit4($"Line Start Color##{colliderType}", ref rendererConfig.LineStartColor))
                    {
                        rendererConfig.LineEndColor.W = rendererConfig.LineStartColor.W;
                        CollidersRenderPatches.UpdateDrawLines(colliderType);
                    }

                    if (ImGui.ColorEdit4($"Line End Color##{colliderType}", ref rendererConfig.LineEndColor))
                    {
                        rendererConfig.LineStartColor.W = rendererConfig.LineEndColor.W;
                        CollidersRenderPatches.UpdateDrawLines(colliderType);
                    }

                    if (ImGui.ColorEdit4($"Surface Color##{colliderType}", ref rendererConfig.SurfaceColor))
                    {
                        CollidersRenderPatches.UpdateDrawLines(colliderType);
                    }

                    if (colliderType == ColliderType.Other && !CollidersRenderPatches.EnableOtherColliders)
                    {
                        ImGui.EndDisabled();
                    }
                });
            }
        }

        // Other options
        if (ImGui.CollapsingHeader("Others"))
        {
            ImGui.SeparatorText("Menu");

            WithDisabled(!SceneUtils.IsGameScene, () =>
            {
                if (ImGui.Checkbox("Hide HUD", ref OtherPatches.HideHud))
                {
                    UiHelpers.ToggleHudVisibility(!OtherPatches.HideHud);
                }
            });

            ImGui.NewLine();

            WithDisabled(!SceneUtils.IsGameScene, () =>
            {
                if (ImGui.Button("Return to statue"))
                {
                    SceneUtils.ReturnToStatue();
                }
            });
            ImGui.SameLine();
            if (ImGui.Button("Return to title screen"))
            {
                SceneUtils.ReturnToTitleScreen();
            }

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
                if (ImGui.Button("Update##GameCleared"))
                {
                    OtherPatches.UpdateGameCleared();
                }
                ImGui.SameLine();
                ImGui.InputInt("NG+ Count", ref OtherPatches.GameCleared);

                ImGui.NewLine();
                if (ImGui.Checkbox("Show Teleport menu", ref OtherPatches.ForceShowTeleportMenu))
                {
                    OtherPatches.SetShowTeleportMenu();
                }

                ImGui.SeparatorText("Scene");
                ImGui.Combo("Game Stage", ref OtherPatches.NextGameStageIndex, OtherPatches.AvailableGameScenes,
                    OtherPatches.AvailableGameScenes.Length);
                ImGui.InputInt("Save Point", ref OtherPatches.NextSavePoint);

                if (ImGui.Button("Load scene"))
                {
                    UiHelpers.ForceCloseAllUi();

                    Game.SwitchScene(new SceneSwitchData(OtherPatches.NextGameStageIndex, OtherPatches.NextSavePoint, false));
                }
            }
        }

        ImGui.End();
    }
}