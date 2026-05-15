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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OpenApi { get; set; } = "3.2.0";

        /// <summary>
        /// Specifies the Swagger Specification version being used. It can be used by the Swagger UI and other clients to interpret the API listing. The value MUST be "2.0".
        /// </summary>
        [JsonPropertyName("swagger")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Swagger { get; set; }

        /// <summary>
        /// The host (name or ip) serving the API.
        /// </summary>
        [JsonPropertyName("host")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Host { get; set; }

        /// <summary>
        /// The base path on which the API is served, which is relative to the host.
        /// </summary>
        [JsonPropertyName("basePath")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BasePath { get; set; }

        /// <summary>
        /// The transfer protocol of the API.
        /// </summary>
        [JsonPropertyName("schemes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Schemes { get; set; }

        /// <summary>
        /// A list of MIME types the APIs can consume.
        /// </summary>
        [JsonPropertyName("consumes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Consumes { get; set; }

        /// <summary>
        /// A list of MIME types the APIs can produce.
        /// </summary>
        [JsonPropertyName("produces")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Produces { get; set; }

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
        /// Swagger 2.0 definitions.
        /// </summary>
        [JsonPropertyName("definitions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? Definitions { get; set; }

        /// <summary>
        /// Swagger 2.0 parameters.
        /// </summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiParameter>? Parameters { get; set; }

        /// <summary>
        /// Swagger 2.0 responses.
        /// </summary>
        [JsonPropertyName("responses")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiResponse>? Responses { get; set; }

        /// <summary>
        /// Swagger 2.0 securityDefinitions.
        /// </summary>
        [JsonPropertyName("securityDefinitions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSecurityScheme>? SecurityDefinitions { get; set; }

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
        public IList<OpenApiTag>? Tags { get; set; }

        /// <summary>
        /// Additional external documentation.
        /// </summary>
        [JsonPropertyName("externalDocs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiExternalDocs? ExternalDocs { get; set; }
    }
}
