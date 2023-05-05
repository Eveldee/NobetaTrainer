using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using EnumsNET;
using Humanizer;
using Il2CppInterop.Runtime;
using ImGuiNET;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer
{
    public class TrainerOverlay : Overlay
    {
        private static readonly Vector4 ValueColor = new(252 / 255f, 161 / 255f, 3 / 255f, 1f);
        private static readonly Vector4 InfoColor = new(3 / 255f, 148 / 255f, 252 / 255f, 1f);

        private readonly string[] AvailableSkins = Enum.GetNames<GameSkin>();

        private bool _showImGuiAboutWindow;
        private bool _showImGuiStyleEditorWindow;
        private bool _showImGuiDebugLogWindow;
        private bool _showImGuiDemoWindow;
        private bool _showImGuiMetricsWindow;
        private bool _showImGuiUserGuideWindow;
        private bool _showImGuiStackToolWindow;

        private bool _noDamageEnabled;
        public bool NoDamageEnabled => _noDamageEnabled;

        private bool _infiniteHpEnabled;
        public bool InfiniteHpEnabled => _infiniteHpEnabled;

        private bool _infiniteManaEnabled;
        public bool InfiniteManaEnabled => _infiniteManaEnabled;

        private bool _infiniteStaminaEnabled;
        public bool InfiniteStaminaEnabled => _infiniteStaminaEnabled;

        private bool _forceShowTeleportMenu;
        public bool ForceShowTeleportMenu
        {
            get => _forceShowTeleportMenu;
            set => _forceShowTeleportMenu = value;
        }

        private int _soulsCount;
        public int SoulsCount
        {
            get => _soulsCount;
            set => _soulsCount = value;
        }

        private int _activeSkinIndex;

        private int _arcaneMagicLevel;
        private int _iceMagicLevel;
        private int _fireMagicLevel;
        private int _thunderMagicLevel;
        private int _windMagicLevel;
        private int _absorbMagicLevel;

        private bool _oneTapEnabled;
        public bool OneTapEnabled => _oneTapEnabled;

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
                HelpMarker("Ignore damages, disabling any effect like knockback");

                ImGui.Checkbox("Infinite HP", ref _infiniteHpEnabled);
                HelpMarker("Regen HP anytime it goes below max");

                ImGui.Checkbox("Infinite Mana", ref _infiniteManaEnabled);
                HelpMarker("Regen Mana anytime it goes below max");

                ImGui.Checkbox("Infinite Stamina", ref _infiniteStaminaEnabled);
                HelpMarker("Regen Stamina anytime it goes below max");

                ImGui.SeparatorText("Items");
                ImGui.DragInt("Souls", ref _soulsCount, 10, 0, 99_999);
                ImGui.SameLine();
                if (ImGui.Button("Set") && WizardGirlManagePatches.Instance is not null)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        Game.GameSave.stats.currentMoney = _soulsCount;
                        Game.UpdateMoney(_soulsCount);
                    });
                }

                ImGui.SeparatorText("Appearance");

                ImGui.Combo("Active Skin", ref _activeSkinIndex, AvailableSkins, AvailableSkins.Length);
                if (ImGui.Button("Load Skin") && WizardGirlManagePatches.Instance is { } wizardGirlManage)
                {
                    var gameSkin = (GameSkin) _activeSkinIndex;

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        wizardGirlManage.PreloadSkin(gameSkin);
                        var assetKey = wizardGirlManage.GetSkinAssetKey(gameSkin);

                        var nobetaSkin = Addressables.LoadAsset<GameObject>(assetKey).WaitForCompletion();

                        wizardGirlManage.ReplaceActiveSkin(gameSkin);
                    });
                }

                ImGui.SeparatorText("Actions");
            }

            // Magic options
            if (ImGui.CollapsingHeader("Magic"))
            {
                if (UiGameSavePatches.CurrentGameSave is not { } gameSave)
                {
                    ImGui.Text("No save loaded...");
                }
                else
                {
                    if (ImGui.DragInt("Arcane Level", ref _arcaneMagicLevel, 0.1f, 1, 5))
                    {
                        gameSave.stats.secretMagicLevel = _arcaneMagicLevel;
                    }
                    if (ImGui.DragInt("Ice Level", ref _iceMagicLevel, 0.1f, 1, 5))
                    {
                        gameSave.stats.iceMagicLevel = _iceMagicLevel;
                    }
                    if (ImGui.DragInt("Fire Level", ref _fireMagicLevel, 0.1f, 1, 5))
                    {
                        gameSave.stats.fireMagicLevel = _fireMagicLevel;
                    }
                    if (ImGui.DragInt("Thunder Level", ref _thunderMagicLevel, 0.1f, 1, 5))
                    {
                        gameSave.stats.thunderMagicLevel = _thunderMagicLevel;
                    }
                    if (ImGui.DragInt("Wind Level", ref _windMagicLevel, 0.1f, 1, 5))
                    {
                        gameSave.stats.windMagicLevel = _windMagicLevel;
                    }
                    if (ImGui.DragInt("Absorption Level", ref _absorbMagicLevel, 0.1f, 1, 5))
                    {
                        gameSave.stats.manaAbsorbLevel = _absorbMagicLevel;
                    }
                }

            }

            if (ImGui.CollapsingHeader("Combat"))
            {
                ImGui.SeparatorText("General");

                ImGui.Checkbox("One Tap", ref _oneTapEnabled);
                HelpMarker("Kill all enemies in one hit, effectively deals just a stupid amount of damage");
            }

            if (ImGui.CollapsingHeader("Others"))
            {
                ImGui.SeparatorText("Environment");

                if (ImGui.Button("Remove Lava"))
                {
                    var gameObjects = Object.FindObjectsOfType<GameObject>();

                    foreach (var gameObject in gameObjects)
                    {
                        if (gameObject.name.Contains("Lava"))
                        {
                            Plugin.Log.LogDebug(gameObject.name);
                        }

                        // Visual Lava
                        if (EnvironmentUtils.LavaTrapNamePrefix.Any(prefix => gameObject.name.StartsWith(prefix)))
                        {
                            Object.Destroy(gameObject);
                        }
                    }
                }

                ImGui.SeparatorText("Save");

                if (UiGameSavePatches.CurrentGameSave is { } gameSave)
                {
                    if (ImGui.Checkbox("Show Teleport menu", ref _forceShowTeleportMenu))
                    {
                        gameSave.basic.showTeleportMenu = _forceShowTeleportMenu;
                        Plugin.Log.LogDebug(_forceShowTeleportMenu);
                    }
                }
                else
                {
                    ImGui.Text("Please load a save first...");
                }
            }

            ImGui.End();
        }

        private void ShowInspectWindow()
        {
            ImGui.Begin("NobetaTrainerInspector");
            ImGui.PushTextWrapPos();

            ImGui.TextColored(InfoColor, "Notes:");
            ImGui.Text("- Vectors are shown in (x, y, z) format");

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

            if (ImGui.CollapsingHeader("PlayerStats"))
            {
                if (UiGameSavePatches.CurrentGameSave?.stats is not { } stats)
                {
                    ImGui.TextWrapped("No stats available, load a save first...");
                }
                else
                {
                    ImGui.SeparatorText("General");

                    ShowValue("Health Point:", stats.currentHealthyPoint);
                    ShowValue("Mana point:", stats.currentManaPoint);
                    ShowValue("Magic Index:", stats.currentMagicIndex);
                    ShowValue("Souls:", stats.currentMoney);
                    ShowValue("Curse Percent:", stats.cursePercent);

                    ImGui.SeparatorText("Stats Levels");
                    ShowValue("Health (HP) Level:", stats.healthyLevel);
                    ShowValue("Mana (MP) Level:", stats.manaLevel);
                    ShowValueExpression(stats.staminaLevel);
                    ShowValueExpression(stats.strengthLevel);
                    ShowValueExpression(stats.intelligenceLevel);
                    ShowValue("Haste Level:", stats.dexterityLevel);

                    ImGui.SeparatorText("Magic Levels");
                    ShowValue("Arcane Level:", stats.secretMagicLevel);
                    ShowValue("Ice Level:", stats.iceMagicLevel);
                    ShowValue("Fire Level:", stats.fireMagicLevel);
                    ShowValue("Thunder Level:", stats.thunderMagicLevel);
                    ShowValue("Wind Level:", stats.windMagicLevel);
                    ShowValue("Mana Absorb Level:", stats.manaAbsorbLevel);
                }
            }

            if (ImGui.CollapsingHeader("Save Basic Data"))
            {
                if (UiGameSavePatches.CurrentGameSave?.basic is not { } basicData)
                {
                    ImGui.TextWrapped("No save loaded, load a save first...");
                }
                else
                {
                    ImGui.SeparatorText("General");
                    ShowValue("Save Slot:", basicData.dataIndex);
                    ShowValueExpression(basicData.difficulty);
                    ShowValue("Game Cleared Times:", basicData.gameCleared);
                    ShowValue("Gaming Time:", TimeSpan.FromSeconds(basicData.gamingTime).ToString(FormatUtils.TimeSpanSecondesFormat));
                    ShowValueExpression(Game.GameSave.dataVersion);
                    ShowValue("Last Save:", new DateTime(basicData.timeStamp).ToLocalTime().Humanize());

                    ImGui.SeparatorText("Stages");
                    ShowValueExpression(basicData.stage);
                    ShowValue("Stages Unlocked:", basicData.savePointMap.Count);
                    ShowValueExpression(basicData.savePoint);
                    ShowValueExpression(basicData.showTeleportMenu);

                    ImGui.SeparatorText("Save Points");
                    var savePointMap = basicData.savePointMap;

                    foreach (var savePoint in savePointMap)
                    {
                        ShowValue($"{savePoint.Key}:", $"{string.Join(", ", savePoint.Value._items.Take(savePoint.Value.Count))}");
                    }
                }
            }

            if (ImGui.CollapsingHeader("Wizard Girl Manage"))
            {
                if (WizardGirlManagePatches.Instance is not { } wizardGirl)
                {
                    ImGui.Text("No character loaded...");
                }
                else
                {
                    if (ImGui.TreeNode("General"))
                    {
                        ImGui.SeparatorText("Position");
                        ShowValue("Position:", wizardGirl.g_PlayerCenter.position.Format());
                        ShowValue("Center  :", wizardGirl.GetCenter().Format());
                        ShowValue("AimTarget:", wizardGirl.aimTarget.position.Format());

                        ImGui.SeparatorText("Status");
                        ShowValueExpression(wizardGirl.GetPlayerStatus());
                        ShowValueExpression(wizardGirl.currentActiveSkin);
                        ShowValueExpression(wizardGirl.GetIsDead());
                        ShowValue("Stealth:", wizardGirl.g_bStealth);
                        ShowValueExpression(wizardGirl.GetRadius());

                        ImGui.SeparatorText("Magic");
                        ShowValueExpression(wizardGirl.GetMagicType() == PlayerEffectPlay.Magic.Null ? "Arcane" : wizardGirl.GetMagicType());
                        ShowValue("Charging:", wizardGirl.GetIsChanging());
                        ShowValueExpression(wizardGirl.IsChargeMax());
                        ShowValue("Player Shot Effect:", wizardGirl.g_bPlayerShotEffect);

                        ImGui.SeparatorText("Items");
                        ShowValue("Item Slots:", wizardGirl.g_PlayerItem.g_iItemSize);
                        ShowValue("Max Item Slots:", wizardGirl.g_PlayerItem.GetItemSizeMax());

                        ShowValue("Hold Item:", wizardGirl.g_PlayerItem.g_HoldItem.Humanize());
                        ShowValue("Item Using:", wizardGirl.g_PlayerItem.g_ItemUsing);

                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Base Data"))
                    {
                        var basic = wizardGirl.BaseData;

                        ImGui.SeparatorText("Charge");
                        ShowValueExpression(basic.g_fCharge);
                        ShowValueExpression(basic.g_bChargeing);
                        ShowValueExpression(basic.g_bChargeFadeStop);
                        ShowValueExpression(basic.g_fChargeAddVal);
                        ShowValueExpression(basic.g_fChargeFade);
                        ShowValueExpression(basic.g_fChargeMax);
                        ShowValueExpression(basic.g_fChargeSpeed);
                        ShowValueExpression(basic.g_fChargeWait);
                        ShowValueExpression(basic.g_fChargeWaitVal);

                        ImGui.SeparatorText("Status");
                        ShowValueExpression(basic.isPlayer);
                        ShowValueExpression(basic.g_bIsTired);
                        ShowValueExpression(basic.NeedResetAppearanceTimer);

                        ImGui.SeparatorText("Cooldowns");
                        ShowValue("CD Arcane:", basic.g_fCDNull);
                        ShowValueExpression(basic.g_fCDIce);
                        ShowValueExpression(basic.g_fCDFire);
                        ShowValueExpression(basic.g_fCDLightning);
                        ShowValue("Arcane CD Scale:", basic.GetCDScale(PlayerEffectPlay.Magic.Null));
                        ShowValue("Ice CD Scale:", basic.GetCDScale(PlayerEffectPlay.Magic.Ice));
                        ShowValue("Fire CD Scale:", basic.GetCDScale(PlayerEffectPlay.Magic.Fire));
                        ShowValue("Lightning CD Scale:", basic.GetCDScale(PlayerEffectPlay.Magic.Lightning));

                        ImGui.SeparatorText("HP");
                        ShowValueExpression(basic.g_fHealthPoints);
                        ShowValueExpression(basic.g_fHP);
                        ShowValueExpression(basic.g_fHPMax);
                        ShowValueExpression(basic.g_fHPRecovery);
                        ShowValueExpression(basic.g_fSecondMultipleEasyHP);
                        ShowValueExpression(basic.g_fSecondMultipleHP);

                        ImGui.SeparatorText("MP");
                        ShowValueExpression(basic.g_fManaPoints);
                        ShowValueExpression(basic.g_fMP);
                        ShowValueExpression(basic.g_fMPMax);
                        ShowValueExpression(basic.g_fMPRecovery);

                        ImGui.SeparatorText("SP");
                        ShowValueExpression(basic.g_fSP);
                        ShowValueExpression(basic.g_fSPMax);
                        ShowValueExpression(basic.g_fSPRecovery);
                        ShowValueExpression(basic.g_fSPRecoveryStayTime);
                        ShowValueExpression(basic.g_fSPRecoveryStayTimeVal);
                        ShowValueExpression(basic.g_fStaminaPoints);

                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Magic Data"))
                    {
                        var magic = wizardGirl.g_MData;

                        ImGui.SeparatorText("General");
                        ShowValue("Max Charge Fade:", magic.GetChargeMaxFade());

                        ImGui.SeparatorText("Arcane");
                        ShowValueExpression(magic.g_NullCD);
                        ShowValueExpression(magic.g_NullCharge);
                        ShowValueExpression(magic.g_NullChargeFade);
                        ShowValueExpression(magic.g_NullNorExp);

                        ImGui.SeparatorText("Ice");
                        ShowValueExpression(magic.g_IceCD);
                        ShowValueExpression(magic.g_IceCharge);
                        ShowValueExpression(magic.g_IceChargeFade);
                        ShowValueExpression(magic.g_IceNorExp);

                        ImGui.SeparatorText("Fire");
                        ShowValueExpression(magic.g_FireCD);
                        ShowValueExpression(magic.g_FireCharge);
                        ShowValueExpression(magic.g_FireChargeFade);
                        ShowValueExpression(magic.g_FireNorExp);
                        ShowValueExpression(magic.g_FireAttackExp);

                        ImGui.SeparatorText("Thunder");
                        ShowValueExpression(magic.g_LightningCD);
                        ShowValueExpression(magic.g_LightningCharge);
                        ShowValueExpression(magic.g_LightningChargeFade);
                        ShowValueExpression(magic.g_LightningNorExp);
                        ShowValueExpression(magic.g_LightningDodgeExp);

                        ImGui.SeparatorText("Sky Dodge");
                        ShowValueExpression(magic.g_SkyDodge);
                        ShowValueExpression(magic.g_SkyJumpCD);
                        ShowValueExpression(magic.g_SkyJumpCDTime);
                        ShowValueExpression(magic.g_SkyJumpExp);

                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Character Controller"))
                    {
                        var characterController = wizardGirl.characterController;
                        ImGui.SeparatorText("General");

                        ShowValueExpression(characterController.center.Format());
                        ShowValueExpression(characterController.velocity.Format());

                        ImGui.SeparatorText("Status");
                        ShowValueExpression(FlagEnums.FormatFlags(characterController.collisionFlags));
                        ShowValueExpression(characterController.isGrounded);
                        ShowValueExpression(characterController.detectCollisions);
                        ShowValueExpression(characterController.enableOverlapRecovery);

                        ImGui.SeparatorText("Other");
                        ShowValueExpression(characterController.minMoveDistance);
                        ShowValueExpression(characterController.height);
                        ShowValueExpression(characterController.radius);
                        ShowValueExpression(characterController.skinWidth);
                        ShowValueExpression(characterController.slopeLimit);
                        ShowValueExpression(characterController.stepOffset);

                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Camera"))
                    {
                        ImGui.SeparatorText("Camera");

                        ImGui.TreePop();
                    }
                }
            }

            if (ImGui.CollapsingHeader("NobetaRuntimeData"))
            {
                if (WizardGirlManagePatches.RuntimeData is not { } runtimeData)
                {
                    ImGui.TextWrapped("No runtime data available, load a character first...");
                }
                else
                {
                    ImGui.SeparatorText("Constants");
                    ShowValueExpression(NobetaRuntimeData.ABSORB_CD_TIME_MAX, help: "Delay between absorb status");
                    ShowValueExpression(NobetaRuntimeData.ABSORB_STATUS_TIME_MAX, help: "Duration of absorption");
                    ShowValueExpression(NobetaRuntimeData.ABSORB_TIME_MAX, help: "Duration of absorb time status (time in which getting hit triggers an absorption");
                    ShowValueExpression(NobetaRuntimeData.REPULSE_TIME_MAX);
                    ShowValueExpression(NobetaRuntimeData.FULL_TIMER_LIMIT);
                    ShowValueExpression(NobetaRuntimeData.PRAYER_ATTACK_TIME_MAX);

                    ImGui.SeparatorText("Absorb");
                    ShowValueExpression(runtimeData.AbsorbCDTimer);
                    ShowValueExpression(runtimeData.AbsorbStatusTimer);
                    ShowValueExpression(runtimeData.AbsorbTimer);

                    ImGui.SeparatorText("Movement");
                    ShowValueExpression(runtimeData.moveDirection.Format());
                    ShowValueExpression(runtimeData.JumpDirection.Format());
                    ShowValueExpression(runtimeData.previousPosition.Format());
                    ShowValueExpression(runtimeData.moveSpeed);
                    ShowValueExpression(runtimeData.MoveSpeedScale);
                    ShowValueExpression(runtimeData.RotationSpeed);
                    ShowValueExpression(runtimeData.JumpMoveSpeed);
                    ShowValueExpression(runtimeData.JumpForce);

                    ImGui.SeparatorText("Physics");
                    ShowValueExpression(runtimeData.FallSpeedMax);
                    ShowValueExpression(runtimeData.FallTimer);
                    ShowValueExpression(runtimeData.Gravity);
                    ShowValueExpression(runtimeData.HardBody);
                    ShowValueExpression(runtimeData.IsPond);
                    ShowValueExpression(runtimeData.PondHeight);
                    ShowValueExpression(runtimeData.IsSky);

                    ImGui.SeparatorText("Combat");
                    ShowValueExpression(runtimeData.NextAttack);
                    ShowValueExpression(runtimeData.NextEndTime);
                    ShowValueExpression(runtimeData.NextTime);
                    ShowValueExpression(runtimeData.AimReadyWight);
                    ShowValueExpression(runtimeData.AimTime);
                    ShowValueExpression(runtimeData.AimWight);
                    ShowValueExpression(runtimeData.airAttackTimer);
                    ShowValueExpression(runtimeData.NextAirAttack);
                    ShowValueExpression(runtimeData.damagedAirStayTimer);
                    ShowValueExpression(runtimeData.DamageDodgeTimer);
                    ShowValueExpression(runtimeData.DodgeDamage);
                    ShowValueExpression(runtimeData.DodgeTimer);
                    ShowValueExpression(runtimeData.HPRecovery);
                    ShowValueExpression(runtimeData.MPRecovery);
                    ShowValueExpression(runtimeData.MPRecoveryExternal);

                    ImGui.SeparatorText("Magic");
                    ShowValueExpression(runtimeData.ShotEffect);
                    ShowValueExpression(runtimeData.ShotTime);
                    ShowValueExpression(runtimeData.NoFireWaitTime);
                    ShowValueExpression(runtimeData.HasMagicLockTargets);
                    ShowValueExpression(runtimeData.HoldingShot);
                    ShowValueExpression(runtimeData.IsChargeEnable);
                    ShowValue("Lock Targets Count:", runtimeData.MagicLockTargets.Count);

                    ImGui.SeparatorText("Others");
                    ShowValueExpression(runtimeData.TimeScale);
                    ShowValueExpression(runtimeData.WaitTime);
                    ShowValueExpression(runtimeData.Controllable);
                    ShowValueExpression(runtimeData.IsDead);
                    ShowValueExpression(runtimeData.PrayerAttackTimer);
                    ShowValueExpression(runtimeData.repulseTimer);
                    ShowValueExpression(runtimeData.StaminaLossDash);
                    ShowValueExpression(runtimeData.StaminaLossDodge);
                    ShowValueExpression(runtimeData.StaminaLossFall);
                    ShowValueExpression(runtimeData.StaminaLossJump);
                }
            }

            ImGui.PushTextWrapPos();
            ImGui.End();
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

        private static void ToggleButton(string title, ref bool valueToToggle)
        {
            if (ImGui.Button(title))
            {
                valueToToggle = !valueToToggle;
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
}