using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiServerVariable.</summary>
    public class OpenApiServerVariable
    {
/// <summary>Auto-generated documentation for Enum.</summary>
        [JsonPropertyName("enum")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Enum { get; set; }

/// <summary>Auto-generated documentation for Default.</summary>
        [JsonPropertyName("default")]
        public string Default { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }
}
