using UnityEngine;

namespace NobetaTrainer.Utils.Extensions;

public static class UnityExtensions
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
}