using System.Text.Json.Serialization;
using NobetaTrainer.Utils;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Config.Models;

public class BoxColliderRendererConfig
{
    [JsonInclude]
    public bool Enable;
    [JsonInclude]
    public bool DrawLines = true;
    [JsonInclude]
    public float LineWidth = 0.05f;
    [JsonInclude]
    public Vector4 LineStartColor = Color.blue.ToVector4();
    [JsonInclude]
    public Vector4 LineEndColor = Color.red.ToVector4();
}