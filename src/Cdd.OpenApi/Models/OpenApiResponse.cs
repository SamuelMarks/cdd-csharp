using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiResponse.</summary>
    public class OpenApiResponse
    {
/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Content.</summary>
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

/// <summary>Auto-generated documentation for Headers.</summary>
        [JsonPropertyName("headers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiHeader>? Headers { get; set; }

/// <summary>Auto-generated documentation for Links.</summary>
        [JsonPropertyName("links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiLink>? Links { get; set; }
    }
}
