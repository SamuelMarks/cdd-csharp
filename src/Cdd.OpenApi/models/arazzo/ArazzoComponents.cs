// <copyright file="ArazzoComponents.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Holds a set of reusable objects for different aspects of the Arazzo Specification (<c>inputs</c>, <c>parameters</c>, <c>successActions</c>, and <c>failureActions</c>).
    /// </summary>
    public class ArazzoComponents
    {
        /// <summary>
        /// Gets or sets reusable JSON Schema 2020-12 objects for workflow inputs.
        /// </summary>
        [JsonPropertyName("inputs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, OpenApiSchema>? Inputs { get; set; }

        /// <summary>
        /// Gets or sets reusable workflow or step parameter definitions.
        /// </summary>
        [JsonPropertyName("parameters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, ArazzoParameter>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets reusable success action definitions.
        /// </summary>
        [JsonPropertyName("successActions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, ArazzoSuccessAction>? SuccessActions { get; set; }

        /// <summary>
        /// Gets or sets reusable failure action definitions.
        /// </summary>
        [JsonPropertyName("failureActions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, ArazzoFailureAction>? FailureActions { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
