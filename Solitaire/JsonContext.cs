using System.Text.Json.Serialization;
using Solitaire.Models;

namespace Solitaire;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    IncludeFields = true,
    WriteIndented = false)]
[JsonSerializable(typeof(PersistentState))]
public partial class JsonContext : JsonSerializerContext
{
    
}