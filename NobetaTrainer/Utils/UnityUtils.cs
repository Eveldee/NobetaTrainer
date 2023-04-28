using UnityEngine;

namespace NobetaTrainer.Utils;

public static class UnityUtils
{
    public static string Format(this Vector3 vector3)
    {
        return $"{{ x: {vector3.x:F3}, y: {vector3.y:F3}, z: {vector3.z:F3} }}";
    }

    public static bool ToBool(this int value) => value switch
    {
        0 => false,
        _ => true
    };
}