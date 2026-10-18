// <copyright file="ArazzoParameter.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Represents a workflow or step parameter binding, or a Reusable Object referencing <c>$components.parameters.&lt;name&gt;</c>.
    /// </summary>
    public class ArazzoParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter. Required unless <see cref="Reference"/> is specified.
        /// </summary>
        [JsonPropertyName("name")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the location of the parameter (<c>"path"</c>, <c>"query"</c>, <c>"querystring"</c>, <c>"header"</c>, or <c>"cookie"</c>).
        /// </summary>
        [JsonPropertyName("in")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? In { get; set; }

        /// <summary>
        /// Gets or sets the constant value, runtime expression, or Reusable Object override value for the parameter.
        /// </summary>
        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Value { get; set; }

        /// <summary>
        /// Gets or sets a runtime expression reference to a reusable parameter in <c>$components.parameters</c>.
        /// </summary>
        [JsonPropertyName("reference")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Reference { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a Reusable Object reference rather than an inline parameter definition.
        /// </summary>
        [JsonIgnore]
        public bool IsReusable => !string.IsNullOrEmpty(Reference);

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
