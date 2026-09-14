using System;
using System.Text.Json.Serialization;
using Solitaire.Models;

namespace Solitaire.Utils;

[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals)]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(Difficulty))]
[JsonSerializable(typeof(DrawMode))]
internal sealed partial class StorageJsonContext : JsonSerializerContext
{
}
