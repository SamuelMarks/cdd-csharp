// <copyright file="OpenApiEncoding.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>A single encoding definition applied to a single schema property or media type item.</summary>
    public class OpenApiEncoding
    {
        /// <summary>Gets or sets the Content-Type for encoding a specific property.</summary>
        [JsonPropertyName("contentType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentType { get; set; }

        /// <summary>Gets or sets a map of additional header metadata to be provided with the property encoding.</summary>
        [JsonPropertyName("headers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiHeader>? Headers { get; set; }

        /// <summary>Gets or sets the serialization style for the property value.</summary>
        [JsonPropertyName("style")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Style { get; set; }

        /// <summary>Gets or sets a value indicating whether array or object properties generate separate parameters for each value.</summary>
        [JsonPropertyName("explode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Explode { get; set; }

        /// <summary>Gets or sets a value indicating whether reserved URI characters are allowed without percent-encoding.</summary>
        [JsonPropertyName("allowReserved")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? AllowReserved { get; set; }

        /// <summary>Gets or sets nested encoding definitions for object properties.</summary>
        [JsonPropertyName("encoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiEncoding>? Encoding { get; set; }

        /// <summary>Gets or sets positional encoding definitions for tuple array items.</summary>
        [JsonPropertyName("prefixEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<OpenApiEncoding>? PrefixEncoding { get; set; }

        /// <summary>Gets or sets the encoding definition for array items.</summary>
        [JsonPropertyName("itemEncoding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiEncoding? ItemEncoding { get; set; }
    }
}
