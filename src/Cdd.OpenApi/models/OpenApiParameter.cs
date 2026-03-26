using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiParameter.</summary>
    public class OpenApiParameter
    {
/// <summary>Auto-generated documentation for Name.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for In.</summary>
        [JsonPropertyName("in")]
        public string In { get; set; } = string.Empty;

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for Required.</summary>
        [JsonPropertyName("required")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Required { get; set; }

/// <summary>Auto-generated documentation for Deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }

/// <summary>Auto-generated documentation for AllowEmptyValue.</summary>
        [JsonPropertyName("allowEmptyValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowEmptyValue { get; set; }

/// <summary>Auto-generated documentation for Style.</summary>
        [JsonPropertyName("style")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Style { get; set; }

/// <summary>Auto-generated documentation for Explode.</summary>
        [JsonPropertyName("explode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Explode { get; set; }

/// <summary>Auto-generated documentation for AllowReserved.</summary>
        [JsonPropertyName("allowReserved")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowReserved { get; set; }

/// <summary>Auto-generated documentation for Schema.</summary>
        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Schema { get; set; }

/// <summary>Auto-generated documentation for Content.</summary>
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

/// <summary>Auto-generated documentation for Example.</summary>
        [JsonPropertyName("example")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Example { get; set; }

/// <summary>Auto-generated documentation for Examples.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiExample>? Examples { get; set; }
    }
}
