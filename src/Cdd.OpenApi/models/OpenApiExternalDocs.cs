using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiExternalDocs.</summary>
    public class OpenApiExternalDocs
    {
/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for Url.</summary>
        [JsonPropertyName("url")]
/// <summary>Auto-generated documentation for Url.</summary>
        public string Url { get; set; } = string.Empty;
    }
}
