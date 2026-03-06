using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiReference.</summary>
    public class OpenApiReference
    {
/// <summary>Auto-generated documentation for Ref.</summary>
        [JsonPropertyName("$ref")]
/// <summary>Auto-generated documentation for Ref.</summary>
        public string Ref { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Summary.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }
}
