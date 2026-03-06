using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiLink.</summary>
    public class OpenApiLink
    {
/// <summary>Auto-generated documentation for OperationRef.</summary>
        [JsonPropertyName("operationRef")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationRef { get; set; }

/// <summary>Auto-generated documentation for OperationId.</summary>
        [JsonPropertyName("operationId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationId { get; set; }

/// <summary>Auto-generated documentation for Parameters.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, object>? Parameters { get; set; }

/// <summary>Auto-generated documentation for RequestBody.</summary>
        [JsonPropertyName("requestBody")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? RequestBody { get; set; }

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for Server.</summary>
        [JsonPropertyName("server")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiServer? Server { get; set; }
    }
}
