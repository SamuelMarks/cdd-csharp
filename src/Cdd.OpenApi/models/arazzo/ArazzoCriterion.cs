// <copyright file="ArazzoCriterion.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Defines an assertion criterion used to evaluate step success criteria or action guard conditions.
    /// </summary>
    public class ArazzoCriterion
    {
        /// <summary>
        /// Gets or sets the runtime expression providing the evaluation context (e.g., <c>$statusCode</c>, <c>$response.body</c>, <c>$message.payload</c>).
        /// </summary>
        [JsonPropertyName("context")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Context { get; set; }

        /// <summary>
        /// Gets or sets the condition expression to evaluate against the context (e.g., <c>$statusCode == 200</c>, <c>^20[0-9]$</c>, <c>$[?(@.id != null)]</c>).
        /// </summary>
        [JsonPropertyName("condition")]
        public string Condition { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the condition engine type (<c>"simple"</c>, <c>"regex"</c>, <c>"jsonpath"</c>, <c>"xpath"</c>, or an <see cref="ArazzoExpressionType"/> object).
        /// </summary>
        [JsonPropertyName("type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ArazzoCriterionType? Type { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
