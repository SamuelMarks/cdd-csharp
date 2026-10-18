// <copyright file="ArazzoSourceDescription.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Describes a source specification (such as an OpenAPI, AsyncAPI, or Arazzo document) that one or more workflows reference.
    /// </summary>
    public class ArazzoSourceDescription
    {
        /// <summary>
        /// Gets or sets the unique name for the source description matching <c>^[A-Za-z0-9_\-]+$</c>.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL or relative URI reference to the source description document.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the protocol/specification type of the source description (<c>"openapi"</c>, <c>"asyncapi"</c>, or <c>"arazzo"</c>).
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
