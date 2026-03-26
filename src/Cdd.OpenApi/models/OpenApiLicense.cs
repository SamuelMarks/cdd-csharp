using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiLicense.</summary>
    public class OpenApiLicense
    {
/// <summary>Auto-generated documentation for Name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Identifier.</summary>
        [JsonPropertyName("identifier")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Identifier { get; set; }

/// <summary>Auto-generated documentation for Url.</summary>
        [JsonPropertyName("url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Url { get; set; }
    }
}
