// <copyright file="OpenApiAny.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cdd.OpenApi.Models
{
    /// <summary>
    /// Represents a strongly-typed discriminated union value for OpenAPI dynamic fields such as <c>example</c>, <c>default</c>, <c>const</c>, <c>enum</c>, and <c>additionalProperties</c>.
    /// </summary>
    [JsonConverter(typeof(OpenApiAnyJsonConverter))]
    public sealed class OpenApiAny : IEquatable<OpenApiAny>
    {
        /// <summary>
        /// Gets the canonical string or raw JSON representation of the value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the JSON value kind of the underlying value.
        /// </summary>
        public JsonValueKind Kind { get; }

        /// <summary>
        /// Gets the associated <see cref="OpenApiSchema"/> when this value represents a schema object (such as in <c>additionalProperties</c>).
        /// </summary>
        public OpenApiSchema? Schema { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiAny"/> class.
        /// </summary>
        /// <param name="value">The string or raw JSON representation of the value.</param>
        /// <param name="kind">The JSON value kind.</param>
        /// <param name="schema">Optional schema when representing a schema-typed union member.</param>
        public OpenApiAny(string value, JsonValueKind kind = JsonValueKind.String, OpenApiSchema? schema = null)
        {
            Value = value;
            Kind = kind;
            Schema = schema;
        }

        /// <summary>
        /// Implicitly converts a string to an <see cref="OpenApiAny"/> instance.
        /// </summary>
        /// <param name="value">The string value to wrap.</param>
        /// <returns>An <see cref="OpenApiAny"/> wrapping the string, or <c>null</c> if the input is <c>null</c>.</returns>
        [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(value))]
        public static implicit operator OpenApiAny?(string? value) =>
            value == null ? null : new OpenApiAny(value, JsonValueKind.String);

        /// <summary>
        /// Implicitly converts a 32-bit integer to an <see cref="OpenApiAny"/> instance.
        /// </summary>
        /// <param name="value">The integer value to wrap.</param>
        /// <returns>An <see cref="OpenApiAny"/> wrapping the integer.</returns>
        public static implicit operator OpenApiAny(int value) =>
            new OpenApiAny(value.ToString(CultureInfo.InvariantCulture), JsonValueKind.Number);

        /// <summary>
        /// Implicitly converts a boolean to an <see cref="OpenApiAny"/> instance.
        /// </summary>
        /// <param name="value">The boolean value to wrap.</param>
        /// <returns>An <see cref="OpenApiAny"/> wrapping the boolean.</returns>
        public static implicit operator OpenApiAny(bool value) =>
            new OpenApiAny(value ? "true" : "false", value ? JsonValueKind.True : JsonValueKind.False);

        /// <summary>
        /// Implicitly converts an <see cref="OpenApiSchema"/> to an <see cref="OpenApiAny"/> instance.
        /// </summary>
        /// <param name="schema">The schema to wrap.</param>
        /// <returns>An <see cref="OpenApiAny"/> wrapping the schema.</returns>
        public static implicit operator OpenApiAny(OpenApiSchema schema) =>
            new OpenApiAny("{}", JsonValueKind.Object, schema);

        /// <summary>
        /// Returns the string representation of this <see cref="OpenApiAny"/> value.
        /// </summary>
        /// <returns>The underlying value string.</returns>
        public override string ToString() => Value;

        /// <summary>
        /// Determines whether this instance and another specified <see cref="OpenApiAny"/> object have the same value.
        /// </summary>
        /// <param name="other">The <see cref="OpenApiAny"/> to compare to this instance.</param>
        /// <returns><c>true</c> if the values are equal; otherwise, <c>false</c>.</returns>
        public bool Equals(OpenApiAny? other) =>
            other != null && Value == other.Value && Kind == other.Kind;

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="OpenApiAny"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj) =>
            obj is OpenApiAny other && Equals(other);

        /// <summary>
        /// Returns the hash code for this <see cref="OpenApiAny"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    }

    /// <summary>
    /// JSON converter for <see cref="OpenApiAny"/> supporting arbitrary JSON primitives, objects, and arrays.
    /// </summary>
    public sealed class OpenApiAnyJsonConverter : JsonConverter<OpenApiAny>
    {
        /// <summary>
        /// Reads and converts the JSON to type <see cref="OpenApiAny"/>.
        /// </summary>
        /// <param name="reader">The UTF-8 JSON reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted <see cref="OpenApiAny"/> value.</returns>
        public override OpenApiAny Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new OpenApiAny(reader.GetString()!, JsonValueKind.String);
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            return new OpenApiAny(doc.RootElement.GetRawText(), doc.RootElement.ValueKind);
        }

        /// <summary>
        /// Writes a specified <see cref="OpenApiAny"/> value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, OpenApiAny value, JsonSerializerOptions options)
        {
            if (value.Kind == JsonValueKind.String)
            {
                writer.WriteStringValue(value.Value);
            }
            else
            {
                writer.WriteRawValue(value.Value);
            }
        }
    }
}
