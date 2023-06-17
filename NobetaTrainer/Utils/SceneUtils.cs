using System.Linq;

namespace NobetaTrainer.Utils;

public static class SceneUtils
{
    public static bool IsLoading()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Loader";
    }

    public static AreaCheck FindLastAreaCheck()
    {
        var sceneHides = UnityUtils.FindComponentsByTypeForced<SceneIsHide>().Where(hide => !hide.g_bIsHide);
        var areaChecks = UnityUtils.FindComponentsByTypeForced<AreaCheck>();

        var checks = areaChecks.Where(areaCheck => sceneHides.All(hide =>
            areaCheck.ShowArea.Any(area => hide.gameObject.GetInstanceID() == area.GetInstanceID()))
        );

        return checks.First(check => check.ShowArea.Count == sceneHides.Count());
    }
}