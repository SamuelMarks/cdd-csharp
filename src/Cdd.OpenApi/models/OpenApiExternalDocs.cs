// <copyright file="OpenApiExternalDocs.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Allows referencing an external resource for extended documentation.</summary>
    public class OpenApiExternalDocs
    {
        /// <summary>Gets or sets a description of the target documentation.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets the URL for the target documentation.</summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
