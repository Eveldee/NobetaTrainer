﻿using System.Linq;
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

    public static Color ToColor(this System.Numerics.Vector3 vector3) => new(vector3.X, vector3.Y, vector3.Z);
    public static System.Numerics.Vector3 ToVector3(this Color color) => new(color.r, color.g, color.b);

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

    public static T FindComponentByNameForced<T>(string name)
    {
        var gameObject = FindGameObjectByNameForced(name);

        return gameObject is not null ? FindGameObjectByNameForced(name).GetComponent<T>() : default;
    }
}