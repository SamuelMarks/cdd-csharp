// <copyright file="OpenApiOperation.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Describes a single API operation on a path.</summary>
    public class OpenApiOperation
    {
        /// <summary>Gets or sets a list of tags for API documentation control.</summary>
        [JsonPropertyName("tags")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Tags { get; set; }

        /// <summary>Gets or sets a short summary of what the operation does.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        /// <summary>Gets or sets a verbose explanation of the operation behavior.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets additional external documentation for this operation.</summary>
        [JsonPropertyName("externalDocs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiExternalDocs? ExternalDocs { get; set; }

        /// <summary>Gets or sets a unique string used to identify the operation.</summary>
        [JsonPropertyName("operationId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationId { get; set; }

        /// <summary>A list of MIME types the operation can consume.</summary>
        [JsonPropertyName("consumes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Consumes { get; set; }

        /// <summary>A list of MIME types the operation can produce.</summary>
        [JsonPropertyName("produces")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Produces { get; set; }

        /// <summary>The transfer protocol for the operation.</summary>
        [JsonPropertyName("schemes")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<string>? Schemes { get; set; }

        /// <summary>Gets or sets a list of parameters that are applicable for this operation.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiParameter>? Parameters { get; set; }

        /// <summary>Gets or sets the request body applicable for this operation.</summary>
        [JsonPropertyName("requestBody")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiRequestBody? RequestBody { get; set; }

        /// <summary>Gets or sets the list of possible responses as they are returned from executing this operation.</summary>
        [JsonPropertyName("responses")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiResponses? Responses { get; set; }

        /// <summary>Gets or sets a map of possible out-of-band callbacks related to the parent operation.</summary>
        [JsonPropertyName("callbacks")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiCallback>? Callbacks { get; set; }

        /// <summary>Gets or sets a value indicating whether this operation is deprecated.</summary>
        [JsonPropertyName("deprecated")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Deprecated { get; set; }

        /// <summary>Gets or sets a declaration of which security mechanisms can be used for this operation.</summary>
        [JsonPropertyName("security")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<IDictionary<string, IList<string>>>? Security { get; set; }

        /// <summary>Gets or sets an alternative server array to service this operation.</summary>
        [JsonPropertyName("servers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiServer>? Servers { get; set; }
    }
}
