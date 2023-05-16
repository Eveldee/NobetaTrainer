using UnityEngine;

namespace NobetaTrainer.Utils;

// Source: https://stackoverflow.com/a/74441731/7465768
public static class ColliderExtensions
{
    public static bool Contains(this Collider collider, Vector3 worldPosition)
    {
        var direction = collider.bounds.center - worldPosition;
        var ray = new Ray(worldPosition, direction);

        var contains = collider.Raycast(ray, out _, direction.magnitude);

        return contains;
    }
}