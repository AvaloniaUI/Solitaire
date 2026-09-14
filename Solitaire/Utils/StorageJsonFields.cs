using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Solitaire.Utils;

// Read and write the original save fields without reflection.
internal sealed class StorageJsonFields
{
    private readonly Utf8JsonWriter? _writer;
    private readonly JsonElement _source;

    public StorageJsonFields(Utf8JsonWriter writer) => _writer = writer;
    public StorageJsonFields(JsonElement source)
    {
        if (source.ValueKind != JsonValueKind.Object)
            throw new JsonException("Stored settings must be a JSON object.");
        _source = source;
    }

    private bool TryRead(string name, out JsonElement value)
    {
        // Json.NET ignored letter case in property names.
        value = default;
        var found = false;
        foreach (var property in _source.EnumerateObject())
        {
            if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                continue;
            value = property.Value;
            found = true;
        }
        return found;
    }

    public void Object(string name, Action<StorageJsonFields> transfer)
    {
        if (_writer is { } writer)
        {
            writer.WriteStartObject(name);
            transfer(new StorageJsonFields(writer));
            writer.WriteEndObject();
        }
        else if (TryRead(name, out var value) && value.ValueKind != JsonValueKind.Null)
            transfer(new StorageJsonFields(value));
    }

    public void Value<T>(string name, Func<T> get, Action<T> set, JsonTypeInfo<T> metadata)
    {
        if (_writer is { } writer)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, get(), metadata);
        }
        else if (TryRead(name, out var value))
            set(JsonSerializer.Deserialize(value, metadata)!);
    }

    public void EnumValue<T>(string name, Func<T> get, Action<T> set, JsonTypeInfo<T> metadata) where T : struct, Enum
    {
        if (_writer is null && TryRead(name, out var value) && value.ValueKind == JsonValueKind.String)
            set(Enum.Parse<T>(value.GetString()!, true));
        else
            Value(name, get, set, metadata);
    }
}
