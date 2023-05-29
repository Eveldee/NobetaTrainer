using System;
using System.Collections.Generic;
using System.IO;
using NobetaTrainer.Colliders;
using NobetaTrainer.Config.Serialization;

namespace NobetaTrainer.Config;

public class ColliderRendererManager
{
    public Dictionary<ColliderType, BoxColliderRendererConfig> RendererConfigs { get; private set; }

    public string SavePath { get; } = Path.Combine(Plugin.ConfigDirectory.FullName, "ColliderRenderers.json");

    public ColliderRendererManager()
    {
        Load();
    }

    private void Load()
    {
        if (!File.Exists(SavePath))
        {
            RendererConfigs = new Dictionary<ColliderType, BoxColliderRendererConfig>();
        }
        else
        {
            RendererConfigs = SerializeUtils.Deserialize<Dictionary<ColliderType, BoxColliderRendererConfig>>
            (
                File.ReadAllText(SavePath)
            );
        }


        // Add missing types
        foreach (var colliderType in Enum.GetValues<ColliderType>())
        {
            RendererConfigs.TryAdd(colliderType, new BoxColliderRendererConfig());
        }
    }

    public void Save()
    {
        File.WriteAllText(SavePath, SerializeUtils.SerializeIndented(RendererConfigs));
    }
}