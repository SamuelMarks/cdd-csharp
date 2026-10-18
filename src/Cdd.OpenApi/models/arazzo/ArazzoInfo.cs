// <copyright file="ArazzoInfo.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Provides metadata about the Arazzo specification and its workflow suite.
    /// </summary>
    public class ArazzoInfo
    {
        /// <summary>
        /// Gets or sets a human-readable title of the Arazzo specification.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = "Workflows";

        /// <summary>
        /// Gets or sets a short summary of the Arazzo specification.
        /// </summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        /// <summary>
        /// Gets or sets a detailed description of the purpose of the Arazzo workflows (supports CommonMark syntax).
        /// </summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the version string of the Arazzo document (distinct from the Arazzo specification version).
        /// </summary>
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
