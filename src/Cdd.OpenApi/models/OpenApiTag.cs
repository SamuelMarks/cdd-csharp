using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiTag.</summary>
    public class OpenApiTag
    {
/// <summary>Auto-generated documentation for Name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for ExternalDocs.</summary>
        [JsonPropertyName("externalDocs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiExternalDocs? ExternalDocs { get; set; }
    }
}
