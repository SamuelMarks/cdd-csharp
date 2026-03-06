using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiOperation.</summary>
    public class OpenApiOperation
    {
/// <summary>Auto-generated documentation for Tags.</summary>
        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
/// <summary>Auto-generated documentation for Tags.</summary>
        public IList<string>? Tags { get; set; }

/// <summary>Auto-generated documentation for Summary.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

/// <summary>Auto-generated documentation for Description.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

/// <summary>Auto-generated documentation for ExternalDocs.</summary>
        [JsonPropertyName("externalDocs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiExternalDocs? ExternalDocs { get; set; }

/// <summary>Auto-generated documentation for OperationId.</summary>
        [JsonPropertyName("operationId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationId { get; set; }

/// <summary>Auto-generated documentation for Parameters.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
/// <summary>Auto-generated documentation for Parameters.</summary>
        public IList<OpenApiParameter>? Parameters { get; set; }

/// <summary>Auto-generated documentation for RequestBody.</summary>
        [JsonPropertyName("requestBody")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiRequestBody? RequestBody { get; set; }

/// <summary>Auto-generated documentation for Responses.</summary>
        [JsonPropertyName("responses")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiResponses? Responses { get; set; }

/// <summary>Auto-generated documentation for Callbacks.</summary>
        [JsonPropertyName("callbacks")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiCallback>? Callbacks { get; set; }

/// <summary>Auto-generated documentation for Deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }

/// <summary>Auto-generated documentation for Security.</summary>
        [JsonPropertyName("security")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<IDictionary<string, IList<string>>>? Security { get; set; }

/// <summary>Auto-generated documentation for Servers.</summary>
        [JsonPropertyName("servers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
/// <summary>Auto-generated documentation for Servers.</summary>
        public IList<OpenApiServer>? Servers { get; set; }
    }
}
