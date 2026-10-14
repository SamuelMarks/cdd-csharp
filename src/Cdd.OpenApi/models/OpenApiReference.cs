// <copyright file="OpenApiReference.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>A simple object to allow referencing other components in the OpenAPI Description, internally and externally.</summary>
    public class OpenApiReference
    {
        /// <summary>Gets or sets the reference identifier URI.</summary>
        [JsonPropertyName("$ref")]
        public string Ref { get; set; } = string.Empty;

        /// <summary>Gets or sets a short summary which by default SHOULD override that of the referenced component.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        /// <summary>Gets or sets a description which by default SHOULD override that of the referenced component.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }
    }
}
