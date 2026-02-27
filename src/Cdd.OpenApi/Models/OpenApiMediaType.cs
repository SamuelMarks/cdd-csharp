using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiMediaType.</summary>
    public class OpenApiMediaType
    {
/// <summary>Auto-generated documentation for Schema.</summary>
        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Schema { get; set; }

/// <summary>Auto-generated documentation for ItemSchema.</summary>
        [JsonPropertyName("itemSchema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? ItemSchema { get; set; }

/// <summary>Auto-generated documentation for Example.</summary>
        [JsonPropertyName("example")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Example { get; set; }

/// <summary>Auto-generated documentation for Examples.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiExample>? Examples { get; set; }

/// <summary>Auto-generated documentation for Encoding.</summary>
        [JsonPropertyName("encoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiEncoding>? Encoding { get; set; }

/// <summary>Auto-generated documentation for PrefixEncoding.</summary>
        [JsonPropertyName("prefixEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiEncoding>? PrefixEncoding { get; set; }

/// <summary>Auto-generated documentation for ItemEncoding.</summary>
        [JsonPropertyName("itemEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiEncoding? ItemEncoding { get; set; }
    }
}
