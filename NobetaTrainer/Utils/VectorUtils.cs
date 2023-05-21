using System;
using UnityEngine;
using Vector4 = System.Numerics.Vector4;

namespace NobetaTrainer.Utils;

public static class VectorUtils
{
    public static Vector4 AddIntensity(this Vector4 vector4, float intensity)
    {
        var newVector = Vector4.Add(vector4, new Vector4(intensity, intensity, intensity, 1f));

        if (newVector.X > 1f)
        {
            newVector.X = 1f;
        }
        if (newVector.Y > 1f)
        {
            newVector.Y = 1f;
        }
        if (newVector.Z > 1f)
        {
            newVector.Z = 1f;
        }

        return newVector;
    }

    public static Vector4[] IntensityGradient(this Vector4 baseVector, float step, uint stepsCount)
    {
        if (stepsCount < 1)
        {
            return Array.Empty<Vector4>();
        }

        var gradient = new Vector4[stepsCount];
        var lowerSteps = (int)Math.Ceiling((stepsCount - 1) / 2f);
        var higherSteps = (int) Math.Floor((stepsCount - 1) / 2f);

        // Do lower gradient
        for (int i = 0; i < lowerSteps; i++)
        {
            gradient[i] = baseVector.AddIntensity(-(step * (lowerSteps - i)));
        }

        gradient[lowerSteps] = baseVector;

        // Do higher gradient
        for (int i = 0; i < higherSteps; i++)
        {
            gradient[i + lowerSteps + 1] = baseVector.AddIntensity(step * (i + 1));
        }

        return gradient;
    }

    public static Color ToColor(this System.Numerics.Vector3 vector3) => new(vector3.X, vector3.Y, vector3.Z);
    public static System.Numerics.Vector3 ToVector3(this Color color) => new(color.r, color.g, color.b);
    public static Color ToColor(this Vector4 vector4) => new(vector4.X, vector4.Y, vector4.Z, vector4.W);
    public static Vector4 ToVector4(this Color color) => new(color.r, color.g, color.b, color.a);

    public static Vector3 ToUnity(this System.Numerics.Vector3 vector3) => new(vector3.X, vector3.Y, vector3.Z);
    public static Quaternion ToUnity(this System.Numerics.Quaternion quaternion) => new(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);

    public static System.Numerics.Vector3 ToNumerics(this Vector3 vector3) => new(vector3.x, vector3.y, vector3.z);
    public static System.Numerics.Quaternion ToNumerics(this Quaternion quaternion) => new(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
}