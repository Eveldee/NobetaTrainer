using System;
using HarmonyLib;
using NobetaTrainer.Config;
using NobetaTrainer.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NobetaTrainer.Patches;

[Section("Trainer.Movement")]
public static class MovementPatches
{
    [Bind]
    public static bool GlideEnabled;
    [Bind]
    public static float GlideVelocity = 10f;

    public static bool NoClipEnabled;

    private static bool _dashing;
    private static bool _walking;
    private static Vector2 _moveDirection = new(0f, 0f);

    public static void ToggleNoClip()
    {
        // Toggle collider
        Singletons.Dispatcher.Enqueue(() =>
        {
            if (Singletons.CharacterController is { } characterController)
            {
                characterController.enabled = !NoClipEnabled;
            }
        });
    }

    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Move))]
    [HarmonyPrefix]
    private static bool MovePrefix(Vector2 movement)
    {
        _moveDirection = movement;

        // Change character direction only when moving forward
        if (GlideEnabled)
        {
            return movement is { x: 0f, y: 1f } or { x: 0f, y: 0f };
        }

        return true;
    }

    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Dash))]
    [HarmonyPrefix]
    private static void DashPrefix(bool onHolding)
    {
        // Need to hold dash to dash
        if (Singletons.GameSettings.holdDashing)
        {
            _dashing = onHolding;
        }
        // Dash is toggle mode
        else if (onHolding)
        {
            _dashing = !_dashing;
        }
    }

    [HarmonyPatch(typeof(PlayerInputController), nameof(PlayerInputController.Walk))]
    [HarmonyPrefix]
    private static void WalkPrefix(bool onHolding)
    {
        _walking = onHolding;
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Update))]
    [HarmonyPrefix]
    private static void UpdatePrefix()
    {
        if (!GlideEnabled)
        {
            return;
        }

        // Set status to normal to keep control of the camera
        var wizardGirl = Singletons.WizardGirl;
        wizardGirl.playerController.Normal();
        wizardGirl.GetMoveController().verticalForce = 0f;

        // Infinite stamina while in Glide Mode
        var data = wizardGirl.BaseData;
        if (data.g_fSP < data.g_fSPMax)
        {
            data.g_fSP = data.g_fSPMax;
        }

        var controller = Singletons.CharacterController;

        var normalized = new Vector2(_moveDirection.x, _moveDirection.y);
        normalized.Normalize();
        normalized *= Time.deltaTime * GlideVelocity;

        if (_dashing)
        {
            normalized *= 2f;
        }

        if (_walking)
        {
            normalized *= 0.5f;
        }

        // Jump
        float verticalVelocity = 0f;

        if (InputUtils.JumpAction.phase == InputActionPhase.Performed)
        {
            verticalVelocity = Time.deltaTime * GlideVelocity;
        }
        else if (InputUtils.DodgeAction.phase == InputActionPhase.Performed)
        {
            verticalVelocity = -Time.deltaTime * GlideVelocity;
        }

        controller.transform.Translate(normalized.x, verticalVelocity, normalized.y);
    }
}