// <copyright file="ArazzoPayloadReplacement.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Describes a targeted location within a request payload (via JSON Pointer, JSONPath, or XPath) where a dynamic value is injected.
    /// </summary>
    public class ArazzoPayloadReplacement
    {
        /// <summary>
        /// Gets or sets a JSON Pointer, JSONPath, or XPath expression targeting the replacement location within the request payload.
        /// </summary>
        [JsonPropertyName("target")]
        public string Target { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the explicit selector dialect used by <see cref="Target"/> (<c>"jsonpointer"</c>, <c>"jsonpath"</c>, <c>"xpath"</c>, or an <see cref="ArazzoExpressionType"/>) (Arazzo 1.1.0).
        /// </summary>
        [JsonPropertyName("targetSelectorType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ArazzoCriterionType? TargetSelectorType { get; set; }

        /// <summary>
        /// Gets or sets the runtime expression or literal value injected at the <see cref="Target"/> location.
        /// </summary>
        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Value { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
