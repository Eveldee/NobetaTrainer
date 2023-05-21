using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NobetaTrainer.Config.Serialization;

public class NumericsVector4JsonConverter : JsonConverter<Vector4>
{
    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();

        reader.Read();
        var x = reader.GetSingle();
        reader.Read();

        reader.Read();
        var y = reader.GetSingle();
        reader.Read();

        reader.Read();
        var z = reader.GetSingle();
        reader.Read();

        reader.Read();
        var w = reader.GetSingle();

        reader.Read();

        return new Vector4(x, y, z, w);
    }

    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteNumber("W", value.W);

        writer.WriteEndObject();
    }
}