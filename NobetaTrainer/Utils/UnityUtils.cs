using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace NobetaTrainer.Utils;

public static class UnityUtils
{
    public static string Format(this Vector3 vector3)
    {
        return $"({vector3.x:F3}, {vector3.y:F3}, {vector3.z:F3})";
    }

    public static string Format(this Vector2 vector2)
    {
        return $"({vector2.x:F3}, {vector2.y:F3})";
    }

    public static string Format(this Transform transform)
    {
        return $"{{ {transform.name} ({transform.tag}): {transform.position} }}";
    }

    public static string Format(this Quaternion quaternion)
    {
        return $"({quaternion.x:F3}, {quaternion.y:F3}, {quaternion.z:F3}, {quaternion.w:F3})";
    }

    public static bool ToBool(this int value) => value switch
    {
        0 => false,
        _ => true
    };

    public static GameObject FindGameObjectByNameForced(string name)
    {
        return Object.FindObjectsOfType<GameObject>(true)
            .FirstOrDefault(gameObject => gameObject.name == name);
    }

    public static Il2CppArrayBase<TComponent> FindComponentsByTypeForced<TComponent>() where TComponent : Component
    {
        return Object.FindObjectsOfType<TComponent>(true);
    }

    public static TComponent FindComponentByNameForced<TComponent>(string name)
    {
        var gameObject = FindGameObjectByNameForced(name);

        return gameObject is not null ? gameObject.GetComponent<TComponent>() : default;
    }
}