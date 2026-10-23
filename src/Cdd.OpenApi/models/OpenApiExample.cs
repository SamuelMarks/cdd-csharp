// <copyright file="OpenApiExample.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>Represents an example of a media type, parameter, or schema payload.</summary>
    public class OpenApiExample
    {
        /// <summary>Gets or sets a short description of the example.</summary>
        [JsonPropertyName("summary")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Summary { get; set; }

        /// <summary>Gets or sets a long description for the example.</summary>
        [JsonPropertyName("description")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; set; }

        /// <summary>Gets or sets the embedded literal example value.</summary>
        [JsonPropertyName("value")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? Value { get; set; }

        /// <summary>Gets or sets an example of the data structure that MUST be valid according to the relevant Schema Object (OpenAPI 3.2.0).</summary>
        [JsonPropertyName("dataValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public OpenApiAny? DataValue { get; set; }

        /// <summary>Gets or sets an example of the serialized form of the value, including encoding and escaping (OpenAPI 3.2.0).</summary>
        [JsonPropertyName("serializedValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SerializedValue { get; set; }

        /// <summary>Gets or sets a URI that points to the literal example value.</summary>
        [JsonPropertyName("externalValue")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ExternalValue { get; set; }
    }
}
