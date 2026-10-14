// <copyright file="OpenApiComponents.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Holds a set of reusable objects for different aspects of the OpenAPI specification.</summary>
    public class OpenApiComponents
    {
        /// <summary>Gets or sets the reusable <see cref="OpenApiSchema"/> objects.</summary>
        [JsonPropertyName("schemas")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? Schemas { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiResponse"/> objects.</summary>
        [JsonPropertyName("responses")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiResponse>? Responses { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiParameter"/> objects.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiParameter>? Parameters { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiExample"/> objects.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiExample>? Examples { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiRequestBody"/> objects.</summary>
        [JsonPropertyName("requestBodies")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiRequestBody>? RequestBodies { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiHeader"/> objects.</summary>
        [JsonPropertyName("headers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiHeader>? Headers { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiSecurityScheme"/> objects.</summary>
        [JsonPropertyName("securitySchemes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSecurityScheme>? SecuritySchemes { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiLink"/> objects.</summary>
        [JsonPropertyName("links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiLink>? Links { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiCallback"/> objects.</summary>
        [JsonPropertyName("callbacks")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiCallback>? Callbacks { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiPathItem"/> objects.</summary>
        [JsonPropertyName("pathItems")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiPathItem>? PathItems { get; set; }

        /// <summary>Gets or sets the reusable <see cref="OpenApiMediaType"/> objects.</summary>
        [JsonPropertyName("mediaTypes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiMediaType>? MediaTypes { get; set; }
    }
}
