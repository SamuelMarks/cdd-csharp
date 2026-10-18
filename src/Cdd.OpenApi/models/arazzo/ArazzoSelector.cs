// <copyright file="ArazzoSelector.cs" company="Cdd.OpenApi">
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
    /// Represents a structured Selector Object in Arazzo 1.1.0 for extracting dynamic output values using JSONPath, XPath, or JSON Pointer.
    /// </summary>
    public class ArazzoSelector
    {
        /// <summary>
        /// Gets or sets the source data context runtime expression (e.g., <c>$response.body</c> or <c>$message.payload</c>).
        /// </summary>
        [JsonPropertyName("context")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Context { get; set; }

        /// <summary>
        /// Gets or sets the query selector expression (e.g., <c>$.items[0].id</c>, <c>/items/0/id</c>, or <c>//item[1]/@id</c>).
        /// </summary>
        [JsonPropertyName("selector")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Selector { get; set; }

        /// <summary>
        /// Gets or sets the query selector dialect (<c>"jsonpath"</c>, <c>"xpath"</c>, <c>"jsonpointer"</c>, or a versioned <see cref="ArazzoExpressionType"/>).
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

    /// <summary>
    /// Represents a strongly-typed discriminated union for a step output entry, which can be either a string runtime expression or a structured <see cref="ArazzoSelector"/> object.
    /// </summary>
    [JsonConverter(typeof(ArazzoStepOutputJsonConverter))]
    public sealed class ArazzoStepOutput : IEquatable<ArazzoStepOutput>
    {
        /// <summary>
        /// Gets the string runtime expression when the output is defined as a string literal; otherwise <c>null</c>.
        /// </summary>
        public string? Expression { get; }

        /// <summary>
        /// Gets the structured <see cref="ArazzoSelector"/> when the output is defined as a Selector Object; otherwise <c>null</c>.
        /// </summary>
        public ArazzoSelector? Selector { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArazzoStepOutput"/> class with a string runtime expression.
        /// </summary>
        /// <param name="expression">The runtime expression string.</param>
        public ArazzoStepOutput(string expression)
        {
            Expression = expression ?? string.Empty;
            Selector = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArazzoStepOutput"/> class with a structured <see cref="ArazzoSelector"/>.
        /// </summary>
        /// <param name="selector">The selector object.</param>
        public ArazzoStepOutput(ArazzoSelector selector)
        {
            Selector = selector ?? new ArazzoSelector();
            Expression = null;
        }

        /// <summary>
        /// Implicitly converts a string runtime expression to an <see cref="ArazzoStepOutput"/>.
        /// </summary>
        /// <param name="expression">The runtime expression string.</param>
        /// <returns>A new <see cref="ArazzoStepOutput"/> instance.</returns>
        public static implicit operator ArazzoStepOutput(string expression) => new ArazzoStepOutput(expression);

        /// <summary>
        /// Implicitly converts an <see cref="ArazzoSelector"/> to an <see cref="ArazzoStepOutput"/>.
        /// </summary>
        /// <param name="selector">The selector object.</param>
        /// <returns>A new <see cref="ArazzoStepOutput"/> instance.</returns>
        public static implicit operator ArazzoStepOutput(ArazzoSelector selector) => new ArazzoStepOutput(selector);

        /// <summary>
        /// Determines whether the specified <see cref="ArazzoStepOutput"/> is equal to the current instance.
        /// </summary>
        /// <param name="other">The other <see cref="ArazzoStepOutput"/> to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
        public bool Equals(ArazzoStepOutput? other)
        {
            if (other is null) return false;
            if (Expression != null || other.Expression != null)
            {
                return string.Equals(Expression, other.Expression, StringComparison.Ordinal);
            }

            return string.Equals(Selector!.Context, other.Selector!.Context, StringComparison.Ordinal) &&
                   string.Equals(Selector.Selector, other.Selector.Selector, StringComparison.Ordinal) &&
                   Equals(Selector.Type, other.Selector.Type);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if equal; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) => Equals(obj as ArazzoStepOutput);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode() => ToString().GetHashCode(StringComparison.Ordinal);

        /// <summary>
        /// Returns a string representation of the step output.
        /// </summary>
        /// <returns>The runtime expression or formatted selector string.</returns>
        public override string ToString()
        {
            if (Expression != null) return Expression;
            var ctx = Selector!.Context ?? "$response.body";
            return $"{ctx}#{Selector.Selector}";
        }
    }

    /// <summary>
    /// Converts <see cref="ArazzoStepOutput"/> instances to and from JSON strings or <see cref="ArazzoSelector"/> JSON objects.
    /// </summary>
    public sealed class ArazzoStepOutputJsonConverter : JsonConverter<ArazzoStepOutput>
    {
        /// <summary>
        /// Shared converter for inner <see cref="ArazzoCriterionType"/> fields.
        /// </summary>
        private static readonly ArazzoCriterionTypeJsonConverter CriterionTypeConverter = new ArazzoCriterionTypeJsonConverter();

        /// <summary>
        /// Reads and converts the JSON token into an <see cref="ArazzoStepOutput"/>.
        /// </summary>
        /// <param name="reader">The UTF-8 JSON reader.</param>
        /// <param name="typeToConvert">The target type to convert.</param>
        /// <param name="options">The serializer options.</param>
        /// <returns>The deserialized <see cref="ArazzoStepOutput"/>.</returns>
        public override ArazzoStepOutput? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return new ArazzoStepOutput(reader.GetString()!);
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;
                var selectorObj = new ArazzoSelector
                {
                    Context = root.TryGetProperty("context", out var cProp) ? cProp.GetString() : null,
                    Selector = root.TryGetProperty("selector", out var sProp) ? sProp.GetString() : null
                };

                if (root.TryGetProperty("type", out var tProp))
                {
                    if (tProp.ValueKind == JsonValueKind.String)
                    {
                        selectorObj.Type = new ArazzoCriterionType(tProp.GetString()!);
                    }
                    else
                    {
                        var innerType = tProp.TryGetProperty("type", out var it) ? it.GetString()! : "jsonpath";
                        var innerVer = tProp.TryGetProperty("version", out var iv) ? iv.GetString() : null;
                        selectorObj.Type = new ArazzoCriterionType(new ArazzoExpressionType { Type = innerType, Version = innerVer });
                    }
                }

                foreach (var prop in root.EnumerateObject())
                {
                    if (prop.Name.StartsWith("x-", StringComparison.Ordinal))
                    {
                        selectorObj.Extensions ??= new Dictionary<string, JsonElement>();
                        selectorObj.Extensions[prop.Name] = prop.Value.Clone();
                    }
                }

                return new ArazzoStepOutput(selectorObj);
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing ArazzoStepOutput.");
        }

        /// <summary>
        /// Writes the <see cref="ArazzoStepOutput"/> value as either a JSON string or a JSON object.
        /// </summary>
        /// <param name="writer">The UTF-8 JSON writer.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="options">The serializer options.</param>
        public override void Write(Utf8JsonWriter writer, ArazzoStepOutput value, JsonSerializerOptions options)
        {
            if (value.Selector != null)
            {
                writer.WriteStartObject();
                if (value.Selector.Context != null)
                {
                    writer.WriteString("context", value.Selector.Context);
                }

                if (value.Selector.Selector != null)
                {
                    writer.WriteString("selector", value.Selector.Selector);
                }

                if (value.Selector.Type != null)
                {
                    writer.WritePropertyName("type");
                    CriterionTypeConverter.Write(writer, value.Selector.Type, options);
                }

                if (value.Selector.Extensions != null)
                {
                    foreach (var ext in value.Selector.Extensions)
                    {
                        writer.WritePropertyName(ext.Key);
                        ext.Value.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }
            else
            {
                writer.WriteStringValue(value.Expression!);
            }
        }
    }
}
