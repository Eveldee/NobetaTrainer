using HarmonyLib;
using Il2CppInterop.Runtime;
using NobetaTrainer.Utils;

namespace NobetaTrainer.Patches;

public static class Singletons
{
    public static NobetaSkin NobetaSkin { get; private set; }
    public static WizardGirlManage WizardGirl { get; private set; }
    public static NobetaRuntimeData RuntimeData { get; private set; }
    public static GameSave GameSave { get; set; }
    public static UnityMainThreadDispatcher Dispatcher => UnityMainThreadDispatcher.Instance;

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

        if (__instance.Pointer == NobetaSkin.Pointer)
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
        RuntimeData = WizardGirl.playerController.runtimeData;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Dispose))]
    [HarmonyPrefix]
    private static void WizardGirlManageDisposePrefix(WizardGirlManage __instance)
    {
        Plugin.Log.LogInfo("WizardGirlManage disposed");

        WizardGirl = null;
        RuntimeData = null;
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
}