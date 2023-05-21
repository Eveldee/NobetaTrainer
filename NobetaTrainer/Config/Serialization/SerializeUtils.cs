using System.Text.Json;
using NobetaTrainer.Config.Serialization;

namespace NobetaTrainer.Serialization;

public static class SerializeUtils
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters =
        {
            new UnityVector3JsonConverter(),
            new UnityQuaternionJsonConverter(),
            new NumericsVector4JsonConverter()
        }
    };

    public static string SerializeIndented<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    public static TValue Deserialize<TValue>(string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, _options);
    }
}