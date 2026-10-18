// <copyright file="ArazzoRequestBody.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Represents a dynamic Request Body Object to be passed to an operation in an Arazzo step.
    /// </summary>
    public class ArazzoRequestBody
    {
        /// <summary>
        /// Gets or sets the Content-Type media type for the request payload (e.g., <c>"application/json"</c>).
        /// </summary>
        [JsonPropertyName("contentType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the payload value or template (object, array, string with embedded expressions, or literal).
        /// </summary>
        [JsonPropertyName("payload")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Payload { get; set; }

        /// <summary>
        /// Gets or sets the list of targeted in-place payload replacements to apply to <see cref="Payload"/> prior to transmission.
        /// </summary>
        [JsonPropertyName("replacements")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<ArazzoPayloadReplacement>? Replacements { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }
}
