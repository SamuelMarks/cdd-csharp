using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>
    /// Represents the root document object of the OpenAPI Specification.
    /// </summary>
    public class OpenApiDocument
    {
        /// <summary>
        /// The semantic version number of the OpenAPI Specification version that the OpenAPI document uses.
        /// </summary>
        [JsonPropertyName("openapi")]
/// <summary>Auto-generated documentation for OpenApi.</summary>
        public string OpenApi { get; set; } = "3.2.0";

        /// <summary>
        /// Provides the self-assigned URI of this document.
        /// </summary>
        [JsonPropertyName("$self")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Self { get; set; }

        /// <summary>
        /// Provides metadata about the API. The metadata MAY be used by tooling as required.
        /// </summary>
        [JsonPropertyName("info")]
        public OpenApiInfo Info { get; set; } = new OpenApiInfo();

        /// <summary>
        /// The default value for the $schema keyword within Schema Objects contained within this OAS document.
        /// </summary>
        [JsonPropertyName("jsonSchemaDialect")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? JsonSchemaDialect { get; set; }

        /// <summary>
        /// An array of Server Objects, which provide connectivity information to a target server.
        /// </summary>
        [JsonPropertyName("servers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
/// <summary>Auto-generated documentation for Servers.</summary>
        public IList<OpenApiServer>? Servers { get; set; }

        /// <summary>
        /// The available paths and operations for the API.
        /// </summary>
        [JsonPropertyName("paths")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiPaths? Paths { get; set; }

        /// <summary>
        /// The incoming webhooks that MAY be received as part of this API and that the API consumer MAY choose to implement.
        /// </summary>
        [JsonPropertyName("webhooks")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiPathItem>? Webhooks { get; set; }

        /// <summary>
        /// An element to hold various Objects for the OpenAPI Description.
        /// </summary>
        [JsonPropertyName("components")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiComponents? Components { get; set; }

        /// <summary>
        /// A declaration of which security mechanisms can be used across the API.
        /// </summary>
        [JsonPropertyName("security")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<IDictionary<string, IList<string>>>? Security { get; set; }

        /// <summary>
        /// A list of tags used by the OpenAPI Description with additional metadata.
        /// </summary>
        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
/// <summary>Auto-generated documentation for Tags.</summary>
        public IList<OpenApiTag>? Tags { get; set; }

        /// <summary>
        /// Additional external documentation.
        /// </summary>
        [JsonPropertyName("externalDocs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiExternalDocs? ExternalDocs { get; set; }
    }
}
