// <copyright file="ArazzoDocument.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Represents the root Arazzo 1.1.0 Specification document describing API workflows and step orchestrations.
    /// </summary>
    public class ArazzoDocument
    {
        /// <summary>
        /// Gets or sets the semantic version number of the Arazzo Specification (e.g., <c>"1.1.0"</c>).
        /// </summary>
        [JsonPropertyName("arazzo")]
        public string Arazzo { get; set; } = "1.1.0";

        /// <summary>
        /// Gets or sets the canonical base URI for resolving relative references within this Arazzo document.
        /// </summary>
        [JsonPropertyName("$self")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Self { get; set; }

        /// <summary>
        /// Gets or sets the metadata about the Arazzo workflows described in this document.
        /// </summary>
        [JsonPropertyName("info")]
        public ArazzoInfo Info { get; set; } = new ArazzoInfo();

        /// <summary>
        /// Gets or sets the list of source descriptions (such as OpenAPI, AsyncAPI, or external Arazzo specifications) referenced by workflows.
        /// </summary>
        [JsonPropertyName("sourceDescriptions")]
        public IList<ArazzoSourceDescription> SourceDescriptions { get; set; } = new List<ArazzoSourceDescription>();

        /// <summary>
        /// Gets or sets the list of executable workflows defined in this Arazzo document.
        /// </summary>
        [JsonPropertyName("workflows")]
        public IList<ArazzoWorkflow> Workflows { get; set; } = new List<ArazzoWorkflow>();

        /// <summary>
        /// Gets or sets the reusable components container for inputs, parameters, success actions, and failure actions.
        /// </summary>
        [JsonPropertyName("components")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ArazzoComponents? Components { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
