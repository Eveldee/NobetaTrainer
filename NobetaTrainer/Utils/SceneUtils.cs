using System.Linq;
using HarmonyLib;

namespace NobetaTrainer.Utils;

public static class SceneUtils
{
    public static bool IsGameScene;

    public static bool IsLoading => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Loader";

    public static AreaCheck FindLastAreaCheck()
    {
        var sceneHides = UnityUtils.FindComponentsByTypeForced<SceneIsHide>().Where(hide => !hide.g_bIsHide);
        var areaChecks = UnityUtils.FindComponentsByTypeForced<AreaCheck>();

        var checks = areaChecks.Where(areaCheck => sceneHides.All(hide =>
            areaCheck.ShowArea.Any(area => hide.gameObject.GetInstanceID() == area.GetInstanceID()))
        );

        return checks.First(check => check.ShowArea.Count == sceneHides.Count());
    }

    public static void ReturnToStatue()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            if (IsGameScene && Singletons.UIPauseMenu is { } pauseMenu)
            {
                pauseMenu.ReloadStage();
            }
        });
    }

    public static void ReturnToTitleScreen()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            UiHelpers.ForceCloseAllUi();
            Game.SwitchTitleScene(false);
        });
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        Plugin.Log.LogDebug($"New scene init complete: {Game.sceneManager.stageName}");

        IsGameScene = true;
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        Plugin.Log.LogDebug("Entered loader scene");

        IsGameScene = false;
    }
}