﻿using System.Text.Json.Serialization;
using NobetaTrainer.Utils.Extensions;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Colliders;

public class BoxColliderRendererConfig
{
    [JsonInclude]
    public bool Enable;
    [JsonInclude]
    public bool DrawLines = true;
    [JsonInclude]
    public bool DrawSurfaces;
    [JsonInclude]
    public float LineWidth = 0.05f;
    [JsonInclude]
    public Vector4 LineStartColor = Color.blue.ToVector4();
    [JsonInclude]
    public Vector4 LineEndColor = Color.red.ToVector4();
    [JsonInclude]
    public Vector4 SurfaceColor = new(127 / 255f, 3 / 255f, 252 / 255f, 20 / 255f);
}