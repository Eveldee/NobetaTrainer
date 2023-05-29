using System.Collections.Generic;
using System.IO;
using System.Linq;
using NobetaTrainer.Config.Serialization;

namespace NobetaTrainer.Teleportation;

public class TeleportationManager
{
    public IEnumerable<TeleportationPoint> TeleportationPoints => SceneName is { } sceneName && _teleportationPoints.ContainsKey(sceneName)
        ? _teleportationPoints[sceneName]
        : Enumerable.Empty<TeleportationPoint>();
    public string SavePath { get; } = Path.Combine(Plugin.ConfigDirectory.FullName, "TeleportationPoints.json");

    private Dictionary<string, List<TeleportationPoint>> _teleportationPoints;

    private static string SceneName => Game.sceneManager?.stageName;

    public TeleportationManager()
    {
        LoadPoints();
    }

    private void LoadPoints()
    {
        if (!File.Exists(SavePath))
        {
            _teleportationPoints = new Dictionary<string, List<TeleportationPoint>>();

            return;
        }

        _teleportationPoints = SerializeUtils.Deserialize<Dictionary<string, List<TeleportationPoint>>>
        (
            File.ReadAllText(SavePath)
        );
    }

    public void SavePoints()
    {
        File.WriteAllText(SavePath, SerializeUtils.SerializeIndented(_teleportationPoints));
    }

    public void AddPoint(TeleportationPoint teleportationPoint)
    {
        // First create empty list if needed
        if (!_teleportationPoints.ContainsKey(SceneName))
        {
            _teleportationPoints[SceneName] = new List<TeleportationPoint>();
        }

        _teleportationPoints[SceneName].Add(teleportationPoint);

        SavePoints();
    }

    public void RemovePoint(TeleportationPoint teleportationPoint)
    {
        _teleportationPoints[SceneName].Remove(teleportationPoint);

        SavePoints();
    }
}