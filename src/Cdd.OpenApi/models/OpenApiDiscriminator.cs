// <copyright file="OpenApiDiscriminator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>
    /// When request bodies or response payloads may be one of a number of different schemas, a discriminator object can be used to aid in serialization, deserialization, and validation.
    /// </summary>
    [JsonConverter(typeof(OpenApiDiscriminatorConverter))]
    public class OpenApiDiscriminator
    {
        /// <summary>
        /// The name of the property in the payload that will hold the discriminator value.
        /// </summary>
        [JsonPropertyName("propertyName")]
        public string PropertyName { get; set; } = string.Empty;

        /// <summary>
        /// An object to hold mappings between payload values and schema names or references.
        /// </summary>
        [JsonPropertyName("mapping")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, string>? Mapping { get; set; }

        /// <summary>
        /// Default mapping fallback
        /// </summary>
        [JsonPropertyName("defaultMapping")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? DefaultMapping { get; set; }
    }

    /// <summary>
    /// Custom JSON converter for OpenApiDiscriminator to support both string and object.
    /// </summary>
    public class OpenApiDiscriminatorConverter : JsonConverter<OpenApiDiscriminator>
    {
        /// <summary>
        /// Reads and converts the JSON to <see cref="OpenApiDiscriminator"/>.
        /// </summary>
        /// <param name="reader">The UTF-8 JSON reader.</param>
        /// <param name="typeToConvert">The target type to convert.</param>
        /// <param name="options">The JSON serializer options.</param>
        /// <returns>The deserialized <see cref="OpenApiDiscriminator"/>.</returns>
        public override OpenApiDiscriminator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new OpenApiDiscriminator { PropertyName = reader.GetString()! };
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var discriminator = new OpenApiDiscriminator();
                using (var document = JsonDocument.ParseValue(ref reader))
                {
                    if (document.RootElement.TryGetProperty("propertyName", out var propName))
                    {
                        discriminator.PropertyName = propName.GetString() ?? string.Empty;
                    }
                    if (document.RootElement.TryGetProperty("mapping", out var mapping))
                    {
                        discriminator.Mapping = JsonSerializer.Deserialize(mapping.GetRawText(), Cdd.OpenApi.Parse.OpenApiJsonContext.Default.DictionaryStringString);
                    }
                    if (document.RootElement.TryGetProperty("defaultMapping", out var defMapping))
                    {
                        discriminator.DefaultMapping = defMapping.GetString();
                    }
                }
                return discriminator;
            }

            throw new JsonException("Expected string or object for Discriminator.");
        }

        /// <summary>
        /// Writes a specified <see cref="OpenApiDiscriminator"/> value as JSON.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer.</param>
        /// <param name="value">The discriminator value to write.</param>
        /// <param name="options">The JSON serializer options.</param>
        public override void Write(Utf8JsonWriter writer, OpenApiDiscriminator value, JsonSerializerOptions options)
        {
            if (value.Mapping == null && value.DefaultMapping == null)
            {
                // Serialize as string for Swagger 2.0 if no 3.x specific properties are used
                writer.WriteStringValue(value.PropertyName);
                return;
            }

            writer.WriteStartObject();
            writer.WriteString("propertyName", value.PropertyName);
            if (value.Mapping != null)
            {
                writer.WritePropertyName("mapping");
                JsonSerializer.Serialize(writer, value.Mapping, Cdd.OpenApi.Parse.OpenApiJsonContext.Default.DictionaryStringString);
            }
            if (value.DefaultMapping != null)
            {
                writer.WriteString("defaultMapping", value.DefaultMapping);
            }
            writer.WriteEndObject();
        }
    }
}
