// <copyright file="ArazzoExpressionType.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models.Arazzo
{
    /// <summary>
    /// Describes an explicit expression dialect and standard version (Criterion Expression Type Object / Expression Type Object in Arazzo 1.1.0).
    /// </summary>
    public class ArazzoExpressionType
    {
        /// <summary>
        /// Gets or sets the expression dialect (<c>"jsonpath"</c>, <c>"xpath"</c>, <c>"jsonpointer"</c>, <c>"simple"</c>, or <c>"regex"</c>).
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "simple";

        /// <summary>
        /// Gets or sets the specific standard version (e.g., <c>"rfc9535"</c>, <c>"draft-goessner-dispatch-jsonpath-00"</c>, <c>"xpath-31"</c>, <c>"xpath-30"</c>, <c>"xpath-20"</c>, <c>"xpath-10"</c>, or <c>"rfc6901"</c>).
        /// </summary>
        [JsonPropertyName("version")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Version { get; set; }

        /// <summary>
        /// Gets or sets specification extension fields (keys prefixed with <c>x-</c>).
        /// </summary>
        [JsonExtensionData]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IDictionary<string, JsonElement>? Extensions { get; set; }
    }

    /// <summary>
    /// Represents a strongly-typed discriminated union for Arazzo <c>type</c> / <c>targetSelectorType</c> fields, which can be serialized either as a string literal or as an <see cref="ArazzoExpressionType"/> object.
    /// </summary>
    [JsonConverter(typeof(ArazzoCriterionTypeJsonConverter))]
    public sealed class ArazzoCriterionType : IEquatable<ArazzoCriterionType>
    {
        /// <summary>
        /// Gets the simple string dialect name (e.g., <c>"simple"</c>, <c>"regex"</c>, <c>"jsonpath"</c>, <c>"xpath"</c>, <c>"jsonpointer"</c>).
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the optional standard version string when a structured expression type object was specified; otherwise <c>null</c>.
        /// </summary>
        public string? Version { get; }

        /// <summary>
        /// Gets the structured <see cref="ArazzoExpressionType"/> object when a versioned expression type object was specified; otherwise <c>null</c>.
        /// </summary>
        public ArazzoExpressionType? ExpressionType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArazzoCriterionType"/> class from a simple dialect string.
        /// </summary>
        /// <param name="type">The dialect name.</param>
        public ArazzoCriterionType(string type)
        {
            Type = type ?? "simple";
            Version = null;
            ExpressionType = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArazzoCriterionType"/> class from a structured <see cref="ArazzoExpressionType"/> object.
        /// </summary>
        /// <param name="expressionType">The structured expression type object.</param>
        public ArazzoCriterionType(ArazzoExpressionType expressionType)
        {
            ExpressionType = expressionType ?? new ArazzoExpressionType();
            Type = ExpressionType.Type;
            Version = ExpressionType.Version;
        }

        /// <summary>
        /// Implicitly converts a string dialect name to an <see cref="ArazzoCriterionType"/>.
        /// </summary>
        /// <param name="type">The string dialect name.</param>
        /// <returns>A new <see cref="ArazzoCriterionType"/> instance.</returns>
        public static implicit operator ArazzoCriterionType(string type) => new ArazzoCriterionType(type);

        /// <summary>
        /// Implicitly converts an <see cref="ArazzoExpressionType"/> to an <see cref="ArazzoCriterionType"/>.
        /// </summary>
        /// <param name="expressionType">The structured expression type object.</param>
        /// <returns>A new <see cref="ArazzoCriterionType"/> instance.</returns>
        public static implicit operator ArazzoCriterionType(ArazzoExpressionType expressionType) => new ArazzoCriterionType(expressionType);

        /// <summary>
        /// Determines whether the specified <see cref="ArazzoCriterionType"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The other <see cref="ArazzoCriterionType"/> to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
        public bool Equals(ArazzoCriterionType? other)
        {
            if (other is null) return false;
            return string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(Version, other.Version, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => Equals(obj as ArazzoCriterionType);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => ToString().ToLowerInvariant().GetHashCode(StringComparison.Ordinal);

        /// <summary>
        /// Returns a string representation of the criterion type.
        /// </summary>
        /// <returns>The dialect string or <c>type:version</c>.</returns>
        public override string ToString()
        {
            var ver = Version;
            return ver != null ? $"{Type}:{ver}" : Type;
        }
    }

    /// <summary>
    /// Converts <see cref="ArazzoCriterionType"/> instances to and from JSON strings or JSON objects.
    /// </summary>
    public sealed class ArazzoCriterionTypeJsonConverter : JsonConverter<ArazzoCriterionType>
    {
        /// <summary>
        /// Reads and converts the JSON token into an <see cref="ArazzoCriterionType"/>.
        /// </summary>
        /// <param name="reader">The UTF-8 JSON reader.</param>
        /// <param name="typeToConvert">The target type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The deserialized <see cref="ArazzoCriterionType"/>.</returns>
        public override ArazzoCriterionType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return new ArazzoCriterionType(reader.GetString()!);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;
                var typeStr = root.TryGetProperty("type", out var tProp) && tProp.ValueKind == JsonValueKind.String
                    ? tProp.GetString()!
                    : "simple";
                var versionStr = root.TryGetProperty("version", out var vProp) && vProp.ValueKind == JsonValueKind.String
                    ? vProp.GetString()
                    : null;
                Dictionary<string, JsonElement>? extensions = null;
                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name.StartsWith("x-", StringComparison.Ordinal))
                    {
                        extensions ??= new Dictionary<string, JsonElement>();
                        extensions[prop.Name] = prop.Value.Clone();
                    }
                }

                return new ArazzoCriterionType(new ArazzoExpressionType
                {
                    Type = typeStr,
                    Version = versionStr,
                    Extensions = extensions
                });
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing ArazzoCriterionType.");
        }

        /// <summary>
        /// Writes the <see cref="ArazzoCriterionType"/> value as either a JSON string or a JSON object.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, ArazzoCriterionType value, JsonSerializerOptions options)
        {
            if (value.ExpressionType != null)
            {
                writer.WriteStartObject();
                writer.WriteString("type", value.ExpressionType.Type);
                if (value.ExpressionType.Version != null)
                {
                    writer.WriteString("version", value.ExpressionType.Version);
                }

                if (value.ExpressionType.Extensions != null)
                {
                    foreach (var ext in value.ExpressionType.Extensions)
                    {
                        writer.WritePropertyName(ext.Key);
                        ext.Value.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStringValue(value.Type);
            }
        }
    }
}
