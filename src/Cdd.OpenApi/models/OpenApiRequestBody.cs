using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiRequestBody.</summary>
    public class OpenApiRequestBody
    {
/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for Content.</summary>
        [JsonPropertyName("content")]
        public IDictionary<string, OpenApiMediaType> Content { get; set; } = new Dictionary<string, OpenApiMediaType>();

/// <summary>Auto-generated documentation for Required.</summary>
        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Required { get; set; }
    }
}
