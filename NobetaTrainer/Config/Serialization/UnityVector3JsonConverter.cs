using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace NobetaTrainer.Serialization;

public class UnityVector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        return new Vector3(x, y, z);
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteNumber("X", value.x);
        writer.WriteNumber("Y", value.y);
        writer.WriteNumber("Z", value.z);

        writer.WriteEndObject();
    }
}