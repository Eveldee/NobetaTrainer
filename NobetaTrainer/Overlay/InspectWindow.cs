using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EnumsNET;
using Humanizer;
using ImGuiNET;
using NobetaTrainer.Patches;
using NobetaTrainer.Utils;
using UnityEngine;

namespace NobetaTrainer.Overlay;

public partial class TrainerOverlay
{
    private static readonly IGrouping<string, PropertyInfo>[] _flagPropertyInfos = typeof(StageFlagData).GetProperties()
        .Where(property => property.Name.StartsWith("Stage", StringComparison.OrdinalIgnoreCase))
        .GroupBy(property => property.Name[..7])
        .ToArray();

    private void ShowInspectWindow()
    {
        ImGui.Begin("Inspector", ref OverlayState.ShowInspectWindow);
        ImGui.PushTextWrapPos();

        ImGui.TextColored(InfoColor, "Notes:");
        ImGui.Text("- Vectors are shown in (x, y, z) format");
        ImGui.Text("- EulerAngles are shown in (x, y, z) format");
        ImGui.Text("- Quaternions are shown in (x, y, z, w) format");

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
            ShowValueExpression(Time.timeScale);

            ImGui.SeparatorText("Cursor");
            ShowValueExpression(Cursor.visible);
            ShowValueExpression(Cursor.lockState);

            ImGui.SeparatorText("Screen");
            ShowValueExpression(Screen.fullScreen);
            ShowValueExpression(Screen.fullScreenMode);
            ShowValueExpression(Screen.height);
            ShowValueExpression(Screen.width);
            ShowValueExpression(Screen.brightness);
            ShowValueExpression(Screen.dpi);
            ShowValueExpression(Screen.orientation);
            ShowValueExpression(Screen.currentResolution);
        }

        if (ImGui.CollapsingHeader("PlayerStats"))
        {
            if (Singletons.GameSave?.stats is not { } stats)
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
            if (Singletons.GameSave?.basic is not { } basicData)
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
            if (Singletons.WizardGirl is not { } wizardGirl)
            {
                ImGui.Text("No character loaded...");
            }
            else
            {
                if (ImGui.TreeNode("General"))
                {
                    ImGui.SeparatorText("Position");
                    ShowValueModifiable(nameof(Transform.position), wizardGirl.transform);
                    ShowValueModifiable(nameof(Transform.rotation), wizardGirl.transform);
                    ShowValueModifiable(nameof(Transform.position), wizardGirl.g_PlayerCenter, "Center");

                    ImGui.NewLine();
                    ShowValueModifiable(nameof(Transform.position), wizardGirl.aimTarget, "AimTarget Position");
                    ShowValueModifiable(nameof(Transform.rotation), wizardGirl.aimTarget, "AimTarget Rotation");

                    ImGui.SeparatorText("Status");
                    ShowValueModifiable(nameof(PlayerController.state), wizardGirl.playerController);
                    #if !V1031
                    ShowValueModifiable(nameof(WizardGirlManage.currentActiveSkin), wizardGirl);
                    #endif
                    ShowValueModifiable(nameof(WizardGirlManage.g_bStealth), wizardGirl, "Stealth");
                    ShowValueExpression(wizardGirl.GetIsDead());
                    ShowValueExpression(wizardGirl.GetRadius());
                    ShowValueExpression(wizardGirl.isNobeta);

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
                    ShowValue("Charge Fade Rate:", wizardGirl.g_MData.GetChargeMaxFade());
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
                    ShowValue("Charge Fade Rate:", magic.GetChargeMaxFade());

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
                    #if !V1031
                    ShowValueExpression(magic.g_SkyDodge);
                    #endif
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
                    ShowValue("Velocity Magnitude", characterController.velocity.magnitude, "F3");

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
            if (Singletons.RuntimeData is not { } runtimeData)
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

        if (ImGui.CollapsingHeader("Stage Flag Data"))
        {
            if (Singletons.GameSave?.flags is { } flags)
            {
                foreach (var grouping in _flagPropertyInfos)
                {
                    TreeNode(grouping.Key.Humanize(LetterCasing.Title), () =>
                    {
                        foreach (var propertyInfo in grouping)
                        {
                            if (ShowValueModifiable(propertyInfo, flags, propertyInfo.Name.Replace('_', ' ').Humanize(LetterCasing.Title)))
                            {
                                Singletons.WizardGirl?.BaseData.UpdateFlag();
                            }
                        }
                    });
                }
            }
        }

        ImGui.PushTextWrapPos();
        ImGui.End();
    }
}