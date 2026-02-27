using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
/// <summary>Auto-generated documentation for OpenApiComponents.</summary>
    public class OpenApiComponents
    {
/// <summary>Auto-generated documentation for Schemas.</summary>
        [JsonPropertyName("schemas")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? Schemas { get; set; }

/// <summary>Auto-generated documentation for Responses.</summary>
        [JsonPropertyName("responses")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiResponse>? Responses { get; set; }

/// <summary>Auto-generated documentation for Parameters.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiParameter>? Parameters { get; set; }

/// <summary>Auto-generated documentation for RequestBodies.</summary>
        [JsonPropertyName("requestBodies")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiRequestBody>? RequestBodies { get; set; }

/// <summary>Auto-generated documentation for Headers.</summary>
        [JsonPropertyName("headers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiHeader>? Headers { get; set; }

/// <summary>Auto-generated documentation for SecuritySchemes.</summary>
        [JsonPropertyName("securitySchemes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSecurityScheme>? SecuritySchemes { get; set; }

/// <summary>Auto-generated documentation for Links.</summary>
        [JsonPropertyName("links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiLink>? Links { get; set; }

/// <summary>Auto-generated documentation for Callbacks.</summary>
        [JsonPropertyName("callbacks")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiCallback>? Callbacks { get; set; }

/// <summary>Auto-generated documentation for PathItems.</summary>
        [JsonPropertyName("pathItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiPathItem>? PathItems { get; set; }

        // TODO: examples, mediaTypes
    }
}
