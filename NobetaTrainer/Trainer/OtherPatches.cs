using System.Linq;
using HarmonyLib;
using NobetaTrainer.Config;
using NobetaTrainer.Utils;
using NobetaTrainer.Utils.Extensions;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace NobetaTrainer.Trainer;

[Section("Others")]
public static class OtherPatches
{
    public static bool ForceShowTeleportMenu;
    public static int GameCleared;

    public static int NextGameStageIndex = 2;
    public static readonly string[] AvailableGameScenes = System.Enum.GetNames<GameStage>();
    public static int NextSavePoint = 1;

    [Bind]
    public static bool BrightMode;
    [Bind]
    public static float BrightModeIntensity = 1f;
    [Bind]
    public static Vector3 BrightModeColor = new(1f, 1f, 1f);
    public static bool HideHud;

    private static Light _light;
    private static float _initialLightIntensity;
    private static Color _initialLightColor;
    private static LightShadows _initialShadows;
    private static bool _initialLightEnabled;

    public static void RemoveLava()
    {
        Singletons.Dispatcher.Enqueue(() =>
        {
            var gameObjects = Object.FindObjectsOfType<GameObject>();

            foreach (var gameObject in gameObjects)
            {
                // Visual Lava
                if (EnvironmentUtils.LavaTrapNamePrefix.Any(prefix => gameObject.name.StartsWith(prefix)))
                {
                    Object.Destroy(gameObject);
                }
            }
        });
    }

    public static void SetShowTeleportMenu()
    {
        if (Singletons.GameSave is not { } gameSave)
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            gameSave.basic.showTeleportMenu = ForceShowTeleportMenu;
        });
    }

    public static void UpdateBrightMode()
    {
        // Here we need to use the overloaded operator because object == null is true for destroyed object
        if (_light == null)
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            var lightBakingOutput = _light.bakingOutput;
            if (BrightMode)
            {
                _light.enabled = true;
                _light.shadows = LightShadows.None;
                lightBakingOutput.lightmapBakeType = LightmapBakeType.Realtime;

                _light.intensity = BrightModeIntensity;
                _light.color = BrightModeColor.ToColor();
            }
            else
            {
                _light.enabled = _initialLightEnabled;
                _light.shadows = _initialShadows;
                lightBakingOutput.lightmapBakeType = LightmapBakeType.Mixed;

                _light.intensity = _initialLightIntensity;
                _light.color = _initialLightColor;
            }

            _light.bakingOutput = lightBakingOutput;
        });
    }

    public static void UpdateGameCleared()
    {
        if (Singletons.GameSave is not { } gameSave)
        {
            return;
        }

        Singletons.Dispatcher.Enqueue(() =>
        {
            gameSave.basic.gameCleared = GameCleared;
        });
    }

    [HarmonyPatch(typeof(UIGameSave), nameof(UIGameSave.StartGamePlay))]
    [HarmonyPostfix]
    private static void StartGamePlayPostfix(GameSave gameSave)
    {
        ForceShowTeleportMenu = Game.GameSave.basic.showTeleportMenu;
        GameCleared = Game.GameSave.basic.gameCleared;
    }

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Enter))]
    [HarmonyPostfix]
    private static void EnterScenePostfix()
    {
        // Use this wrapper because the Light can be deactivated and thus not findable with GameObject.Find
        _light = UnityUtils.FindComponentByNameForced<Light>("Directional Light");

        // Make sure parent GameObject is activated
        _light.gameObject.active = true;

        if (_light is not null)
        {
            _initialShadows = _light.shadows;
            _initialLightColor = _light.color;
            _initialLightIntensity = _light.intensity;
            _initialLightEnabled = _light.enabled;

            UpdateBrightMode();
        }
    }

    // Needed to make sure light intensity doesn't change
    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Update))]
    [HarmonyPrefix]
    private static void Update()
    {
        if (_light != null && BrightMode)
        {
            _light.intensity = BrightModeIntensity;
        }
    }
}