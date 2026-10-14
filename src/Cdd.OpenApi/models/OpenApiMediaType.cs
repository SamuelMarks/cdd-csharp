// <copyright file="OpenApiMediaType.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Provides schema and examples for a particular media type.</summary>
    public class OpenApiMediaType
    {
        /// <summary>Gets or sets the schema defining the content of the request, response, or parameter.</summary>
        [JsonPropertyName("schema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? Schema { get; set; }

        /// <summary>Gets or sets the schema defining the structure of individual items in a streaming or sequential media type.</summary>
        [JsonPropertyName("itemSchema")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiSchema? ItemSchema { get; set; }

        /// <summary>Gets or sets the example of the media type.</summary>
        [JsonPropertyName("example")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Example { get; set; }

        /// <summary>Gets or sets the map of examples for the media type.</summary>
        [JsonPropertyName("examples")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiExample>? Examples { get; set; }

        /// <summary>Gets or sets a map between a property name and its encoding information.</summary>
        [JsonPropertyName("encoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiEncoding>? Encoding { get; set; }

        /// <summary>Gets or sets positional encoding information for multipart or sequential array items.</summary>
        [JsonPropertyName("prefixEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiEncoding>? PrefixEncoding { get; set; }

        /// <summary>Gets or sets encoding information for individual items in a streaming media type.</summary>
        [JsonPropertyName("itemEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiEncoding? ItemEncoding { get; set; }
    }
}
