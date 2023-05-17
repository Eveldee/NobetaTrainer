using HarmonyLib;
using NobetaTrainer.Config;
using UnityEngine;

namespace NobetaTrainer.Behaviours;

[Section("CursorUnlocker")]
public class CursorUnlocker : MonoBehaviour
{
    [Bind]
    public static string ControlPath = "<Keyboard>/alt";
    [Bind]
    public static bool NeedCtrlModifier;
    [Bind]
    public static bool NeedAltModifier;
    [Bind]
    public static bool NeedShiftModifier;

    public static bool IsCursorUnlocked;

    private static bool _currentlySettingCursor;
    private static CursorLockMode _lastLockMode;
    private static bool _lastVisibleState;

    private void Awake()
    {
        Plugin.Log.LogDebug("UnlockCursor Awake");

        _lastLockMode = Cursor.lockState;
        _lastVisibleState = Cursor.visible;
    }

    private void Update()
    {
        _currentlySettingCursor = true;

        if (IsCursorUnlocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = _lastLockMode;
            Cursor.visible = _lastVisibleState;
        }

        _currentlySettingCursor = false;
    }

    [HarmonyPatch(typeof(Cursor), nameof(Cursor.lockState), MethodType.Setter)]
    [HarmonyPrefix]
    private static void CursorLockStateSetPrefix(ref CursorLockMode value)
    {
        if (!_currentlySettingCursor)
        {
            Plugin.Log.LogDebug(value);
            _lastLockMode = value;

            if (IsCursorUnlocked)
            {
                value = CursorLockMode.None;
            }
        }
    }

    [HarmonyPatch(typeof(Cursor), nameof(Cursor.visible), MethodType.Setter)]
    [HarmonyPrefix]
    private static void CursorVisibleSetPrefix(ref bool value)
    {
        if (!_currentlySettingCursor)
        {
            Plugin.Log.LogDebug(value);
            _lastVisibleState = value;

            if (IsCursorUnlocked)
            {
                value = true;
            }
        }
    }
}