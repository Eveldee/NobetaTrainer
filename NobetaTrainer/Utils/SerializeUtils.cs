using System.Text.Json;

namespace NobetaTrainer.Utils;

public static class SerializeUtils
{
    private static JsonSerializerOptions _indentedOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public static string SerializeIndented<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, _indentedOptions);
    }
}