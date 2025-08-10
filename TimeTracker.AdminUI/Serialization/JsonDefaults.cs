// TimeTracker.AdminUI/Serialization/JsonDefaults.cs
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TimeTracker.AdminUI.Serialization;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };
}
