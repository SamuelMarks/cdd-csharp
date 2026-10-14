// <copyright file="OpenApiLink.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Represents a possible design-time link for a response.</summary>
    public class OpenApiLink
    {
        /// <summary>Gets or sets a relative or absolute URI reference to an OAS operation.</summary>
        [JsonPropertyName("operationRef")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationRef { get; set; }

        /// <summary>Gets or sets the name of an existing, resolvable OAS operation, as defined with a unique <c>operationId</c>.</summary>
        [JsonPropertyName("operationId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OperationId { get; set; }

        /// <summary>Gets or sets a map representing parameters to pass to an operation as specified with <c>operationId</c> or identified via <c>operationRef</c>.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiAny>? Parameters { get; set; }

        /// <summary>Gets or sets a literal value or expression to use as a request body when calling the target operation.</summary>
        [JsonPropertyName("requestBody")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? RequestBody { get; set; }

        /// <summary>Gets or sets a description of the link.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets a server object to be used by the target operation.</summary>
        [JsonPropertyName("server")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiServer? Server { get; set; }
    }
}
