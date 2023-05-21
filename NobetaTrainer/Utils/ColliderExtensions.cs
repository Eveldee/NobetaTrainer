using System;
using UnityEngine;

namespace NobetaTrainer.Utils;

public static class ColliderExtensions
{
    // Source: https://stackoverflow.com/a/74441731/7465768
    public static bool Contains(this Collider collider, Vector3 worldPosition)
    {
        var direction = collider.bounds.center - worldPosition;
        var ray = new Ray(worldPosition, direction);

        var contains = collider.Raycast(ray, out _, direction.magnitude);

        return contains;
    }

    public static bool ContainsBox(this BoxCollider boxCollider, Vector3 worldPosition)
    {
        Vector3 OffsetPointWithRotation(Vector3 point)
        {
            return (boxCollider.transform.rotation * point) + boxCollider.transform.position;
        }

        var center = boxCollider.center;

        var firstEdge = OffsetPointWithRotation(center - boxCollider.extents);
        var secondEdge = OffsetPointWithRotation(center + boxCollider.extents);

        var xMin = Math.Min(firstEdge.x, secondEdge.x);
        var xMax = Math.Max(firstEdge.x, secondEdge.x);
        var yMin = Math.Min(firstEdge.y, secondEdge.y);
        var yMax = Math.Max(firstEdge.y, secondEdge.y);
        var zMin = Math.Min(firstEdge.z, secondEdge.z);
        var zMax = Math.Max(firstEdge.z, secondEdge.z);

        return worldPosition.x >= xMin && worldPosition.x <= xMax
                                       && worldPosition.y >= yMin && worldPosition.y <= yMax
                                       && worldPosition.z >= zMin && worldPosition.z <= zMax;
    }
}