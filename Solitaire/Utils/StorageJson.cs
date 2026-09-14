using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Solitaire.ViewModels;

namespace Solitaire.Utils;

public static class StorageJson
{
    public static string Serialize<T>(T value, JsonTypeInfo<T>? metadata = null)
    {
        if (value is null)
            return "null";
        if (value is not CasinoViewModel casino)
            return JsonSerializer.Serialize(value, metadata ?? throw new NotSupportedException("Storage requires generated JSON metadata."));
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            CasinoStorageFields.Transfer(casino, new StorageJsonFields(writer));
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static T? Deserialize<T>(string json, JsonTypeInfo<T>? metadata = null)
    {
        if (typeof(T) != typeof(CasinoViewModel))
            return JsonSerializer.Deserialize(json, metadata ?? throw new NotSupportedException("Storage requires generated JSON metadata."));
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind == JsonValueKind.Null)
            return default;
        var casino = new CasinoViewModel();
        CasinoStorageFields.Transfer(casino, new StorageJsonFields(document.RootElement));
        return (T)(object)casino;
    }
}
