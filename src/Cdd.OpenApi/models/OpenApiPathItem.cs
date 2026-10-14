// <copyright file="OpenApiPathItem.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Describes the operations available on a single path.</summary>
    public class OpenApiPathItem
    {
        /// <summary>Gets or sets an external reference to a Path Item definition.</summary>
        [JsonPropertyName("$ref")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Ref { get; set; }

        /// <summary>Gets or sets a short summary intended to apply to all operations in this path item.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        /// <summary>Gets or sets a description intended to apply to all operations in this path item.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets a definition of a GET operation on this path.</summary>
        [JsonPropertyName("get")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Get { get; set; }

        /// <summary>Gets or sets a definition of a PUT operation on this path.</summary>
        [JsonPropertyName("put")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Put { get; set; }

        /// <summary>Gets or sets a definition of a POST operation on this path.</summary>
        [JsonPropertyName("post")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Post { get; set; }

        /// <summary>Gets or sets a definition of a DELETE operation on this path.</summary>
        [JsonPropertyName("delete")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Delete { get; set; }

        /// <summary>Gets or sets a definition of an OPTIONS operation on this path.</summary>
        [JsonPropertyName("options")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Options { get; set; }

        /// <summary>Gets or sets a definition of a HEAD operation on this path.</summary>
        [JsonPropertyName("head")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Head { get; set; }

        /// <summary>Gets or sets a definition of a PATCH operation on this path.</summary>
        [JsonPropertyName("patch")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Patch { get; set; }

        /// <summary>Gets or sets a definition of a TRACE operation on this path.</summary>
        [JsonPropertyName("trace")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Trace { get; set; }

        /// <summary>Gets or sets a definition of a QUERY operation on this path (OpenAPI 3.2.0).</summary>
        [JsonPropertyName("query")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiOperation? Query { get; set; }

        /// <summary>Gets or sets an alternative server array to service all operations in this path.</summary>
        [JsonPropertyName("servers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiServer>? Servers { get; set; }

        /// <summary>Gets or sets a list of parameters that are applicable for all the operations described under this path.</summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiParameter>? Parameters { get; set; }

        /// <summary>Gets or sets a map of additional custom HTTP method operations on this path (OpenAPI 3.2.0).</summary>
        [JsonPropertyName("additionalOperations")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiOperation>? AdditionalOperations { get; set; }
    }
}
