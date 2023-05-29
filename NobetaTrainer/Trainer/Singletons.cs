using HarmonyLib;
using NobetaTrainer.Behaviours;
using NobetaTrainer.Config;
using NobetaTrainer.Shortcuts;
using NobetaTrainer.Teleportation;
using NobetaTrainer.Timer;
using UnityEngine;

namespace NobetaTrainer.Trainer;

public static class Singletons
{
    public static NobetaSkin NobetaSkin { get; private set; }
    public static WizardGirlManage WizardGirl { get; private set; }
    public static PlayerController PlayerController => WizardGirl?.playerController;
    public static PlayerInputController InputController => WizardGirl?.inputController;
    public static CharacterController CharacterController => WizardGirl?.characterController;
    public static NobetaRuntimeData RuntimeData => WizardGirl?.playerController?.runtimeData;
    public static SceneManager SceneManager => Game.sceneManager;
    public static ItemSystem ItemSystem => SceneManager?.itemSystem;
    public static GameSave GameSave { get; set; }
    public static GameSettings GameSettings => Game.Config?.gameSettings;
    public static StageUIManager StageUi => Game.stageUI;
    public static UnityMainThreadDispatcher Dispatcher => UnityMainThreadDispatcher.Instance;
    public static CursorUnlocker UnlockCursor { get; set; }
    public static ShortcutEditor ShortcutEditor { get; set; }
    public static Timers Timers { get; set; }
    public static TeleportationManager TeleportationManager { get; set; }
    public static ColliderRendererManager ColliderRendererManager { get; set; }

    public static bool SaveLoaded => GameSave is not null;

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.UpdateSkin))]
    [HarmonyPostfix]
    private static void PlayerControllerUpdateSkinPostfix(PlayerController __instance, NobetaSkin skin)
    {
        Plugin.Log.LogInfo("NobetaSkin updated");

        NobetaSkin = skin;
    }

    [HarmonyPatch(typeof(NobetaSkin), nameof(NobetaSkin.Dispose))]
    [HarmonyPrefix]
    private static void NobetaSkinDisposePrefix(NobetaSkin __instance)
    {
        Plugin.Log.LogInfo("NobetaSkin disposed");

        if (__instance.Pointer == NobetaSkin?.Pointer)
        {
            NobetaSkin = null;
        }
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Init))]
    [HarmonyPostfix]
    private static void WizardGirlManageInitPostfix(WizardGirlManage __instance)
    {
        Plugin.Log.LogInfo("WizardGirlManage created");

        WizardGirl = __instance;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Dispose))]
    [HarmonyPrefix]
    private static void WizardGirlManageDisposePrefix(WizardGirlManage __instance)
    {
        Plugin.Log.LogInfo("WizardGirlManage disposed");

        WizardGirl = null;
    }

    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.StartGamePlay))]
    [HarmonyPostfix]
    private static void StartGamePlayPostfix(GameSave gameSave)
    {
        Plugin.Log.LogInfo("Save loaded");

        GameSave = gameSave;
    }

    [HarmonyPatch(typeof(Game), nameof(Game.SwitchTitleScene))]
    [HarmonyPostfix]
    private static void SwitchTitleScenePostfix()
    {
        GameSave = null;
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Enter))]
    [HarmonyPostfix]
    private static void EnterScenePostfix()
    {
        Plugin.Log.LogInfo("Entered scene");
    }
}