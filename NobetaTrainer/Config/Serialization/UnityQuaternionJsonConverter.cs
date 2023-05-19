using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace NobetaTrainer.Serialization;

public class UnityQuaternionJsonConverter : JsonConverter<Quaternion>
{
    public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        return new Quaternion(x, y, z, w);
    }

    public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber("X", value.x);
        writer.WriteNumber("Y", value.y);
        writer.WriteNumber("Z", value.z);
        writer.WriteNumber("W", value.w);

        writer.WriteEndObject();
    }
}