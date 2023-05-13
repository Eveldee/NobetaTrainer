using System;
using System.Numerics;

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
}