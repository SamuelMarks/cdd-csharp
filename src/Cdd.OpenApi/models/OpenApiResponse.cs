// <copyright file="OpenApiResponse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Describes a single response from an API Operation.</summary>
    public class OpenApiResponse
    {
        /// <summary>Gets or sets a short description of the response.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Gets or sets a map containing descriptions of potential response payloads keyed by media type.</summary>
        [JsonPropertyName("content")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiMediaType>? Content { get; set; }

        /// <summary>Swagger 2.0 Schema</summary>
        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Schema { get; set; }

        /// <summary>Gets or sets a map of headers that are sent with the response.</summary>
        [JsonPropertyName("headers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiHeader>? Headers { get; set; }

        /// <summary>Swagger 2.0 examples</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiAny>? Examples { get; set; }

        /// <summary>Gets or sets a map of operations links that can be followed from the response.</summary>
        [JsonPropertyName("links")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiLink>? Links { get; set; }
    }
}
