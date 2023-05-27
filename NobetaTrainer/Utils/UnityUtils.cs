using System.Linq;
using Il2CppInterop.Runtime;
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
        var eulerAngles = quaternion.eulerAngles;
        return $"({eulerAngles.x:F3}, {eulerAngles.y:F3}, {eulerAngles.z:F3})";
    }

    public static bool ToBool(this int value) => value switch
    {
        0 => false,
        _ => true
    };

    public static int ToInt(this bool value) => value switch
    {
        false => 0,
        _ => 1
    };

    public static GameObject FindGameObjectByNameForced(string name)
    {
        #if V1031
        var objects = Object.FindObjectsOfTypeAll(Il2CppType.Of<GameObject>());
        return objects.Cast<Il2CppReferenceArray<GameObject>>().FirstOrDefault(gameObject => gameObject.name == name);
        #else
        return Object.FindObjectsOfType<GameObject>(true)
            .FirstOrDefault(gameObject => gameObject.name == name);
        #endif
    }

    public static Il2CppArrayBase<TComponent> FindComponentsByTypeForced<TComponent>() where TComponent : Component
    {
        #if V1031
        var objects = Object.FindObjectsOfTypeAll(Il2CppType.Of<TComponent>());
        return objects.Cast<Il2CppReferenceArray<TComponent>>();
        #else
        return Object.FindObjectsOfType<TComponent>(true);
        #endif
    }

    public static TComponent FindComponentByNameForced<TComponent>(string name)
    {
        var gameObject = FindGameObjectByNameForced(name);

        return gameObject is not null ? gameObject.GetComponent<TComponent>() : default;
    }
}