using System;
using System.Globalization;
using System.Numerics;

namespace NobetaTrainer.Config.BindConverters;

public class Vector4BindConverter : IBindConverter
{
    private static string GroupSeparator => NumberFormatInfo.CurrentInfo.NumberGroupSeparator;

    public string Serialize(object value)
    {
        if (value is not Vector4 vector4)
        {
            throw new ConvertUnsupportedTypeException(this, value.GetType());
        }

        return vector4.ToString();
    }

    public object Deserialize(string text)
    {
        var span = text.AsSpan();
        var separator = NumberFormatInfo.CurrentInfo.NumberGroupSeparator;

        var vector = span[1..^1];
        var nextSeparatorIndex = vector.IndexOf(GroupSeparator);
        var x = float.Parse(vector[..nextSeparatorIndex]);

        vector = vector[(nextSeparatorIndex + 1)..];
        nextSeparatorIndex = vector.IndexOf(GroupSeparator);
        var y = float.Parse(vector[..nextSeparatorIndex]);

        vector = vector[(nextSeparatorIndex + 1)..];
        nextSeparatorIndex = vector.IndexOf(GroupSeparator);
        var z = float.Parse(vector[..nextSeparatorIndex]);

        vector = vector[(nextSeparatorIndex + 1)..];
        var w = float.Parse(vector);

        return new Vector4(x, y, z, w);
    }
}