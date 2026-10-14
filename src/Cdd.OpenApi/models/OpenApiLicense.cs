// <copyright file="OpenApiLicense.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>License information for the exposed API.</summary>
    public class OpenApiLicense
    {
        /// <summary>Gets or sets the license name used for the API.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets an SPDX license expression for the API.</summary>
        [JsonPropertyName("identifier")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Identifier { get; set; }

        /// <summary>Gets or sets a URL to the license used for the API.</summary>
        [JsonPropertyName("url")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Url { get; set; }
    }
}
