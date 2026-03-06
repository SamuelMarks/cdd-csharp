using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiPathItem.</summary>
    public class OpenApiPathItem
    {
/// <summary>Auto-generated documentation for Ref.</summary>
        [JsonPropertyName("$ref")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Ref { get; set; }

/// <summary>Auto-generated documentation for Summary.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for Get.</summary>
        [JsonPropertyName("get")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Get { get; set; }

/// <summary>Auto-generated documentation for Put.</summary>
        [JsonPropertyName("put")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Put { get; set; }

/// <summary>Auto-generated documentation for Post.</summary>
        [JsonPropertyName("post")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Post { get; set; }

/// <summary>Auto-generated documentation for Delete.</summary>
        [JsonPropertyName("delete")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Delete { get; set; }

/// <summary>Auto-generated documentation for Options.</summary>
        [JsonPropertyName("options")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Options { get; set; }

/// <summary>Auto-generated documentation for Head.</summary>
        [JsonPropertyName("head")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Head { get; set; }

/// <summary>Auto-generated documentation for Patch.</summary>
        [JsonPropertyName("patch")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Patch { get; set; }

/// <summary>Auto-generated documentation for Trace.</summary>
        [JsonPropertyName("trace")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Trace { get; set; }

/// <summary>Auto-generated documentation for Query.</summary>
        [JsonPropertyName("query")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Query { get; set; }

/// <summary>Auto-generated documentation for Servers.</summary>
        [JsonPropertyName("servers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
/// <summary>Auto-generated documentation for Servers.</summary>
        public IList<OpenApiServer>? Servers { get; set; }

/// <summary>Auto-generated documentation for Parameters.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
/// <summary>Auto-generated documentation for Parameters.</summary>
        public IList<OpenApiParameter>? Parameters { get; set; }

        // additionalOperations is a Map, but its key can be dynamic HTTP verbs. We might need a custom JsonConverter later.
/// <summary>Auto-generated documentation for AdditionalOperations.</summary>
        [JsonPropertyName("additionalOperations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiOperation>? AdditionalOperations { get; set; }
    }
}
