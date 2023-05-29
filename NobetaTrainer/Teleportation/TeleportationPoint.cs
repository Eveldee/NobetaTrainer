using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using UnityEngine;

namespace NobetaTrainer.Teleportation;

public class TeleportationPoint
{
    public required string PointName { get; set; }
    public required Vector3 Position { get; set; }
    public required Quaternion Rotation { get; set; }
    public required string AreaCheckName { get; set; }

    [JsonConstructor]
    public TeleportationPoint()
    {

    }

    [SetsRequiredMembers]
    public TeleportationPoint(string pointName, Vector3 position, Quaternion rotation, string areaCheckName)
    {
        PointName = pointName;
        Position = position;
        Rotation = rotation;
        AreaCheckName = areaCheckName;
    }
}