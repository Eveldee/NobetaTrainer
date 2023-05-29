using System;
using UnityEngine;

namespace NobetaTrainer.Utils.Extensions;

public static class ColliderExtensions
{
    // Source: https://stackoverflow.com/a/74441731/7465768
    // Note that this function doesn't work on disabled colliders
    public static bool ContainsRaycast(this Collider collider, Vector3 worldPosition)
    {
        var direction = collider.bounds.center - worldPosition;
        var ray = new Ray(worldPosition, direction);

        var isOutside = collider.Raycast(ray, out _, direction.magnitude);

        return !isOutside && collider.enabled && collider.gameObject.activeSelf;
    }

    public static bool Contains(this BoxCollider boxCollider, Vector3 worldTargetPosition)
    {
        var localPosition = boxCollider.transform.InverseTransformPoint(worldTargetPosition) - boxCollider.center;
        var size = boxCollider.size / 2;

        return MathF.Abs(localPosition.x) <= size.x &&
               MathF.Abs(localPosition.y) <= size.y &&
               MathF.Abs(localPosition.z) <= size.z;
    }
}