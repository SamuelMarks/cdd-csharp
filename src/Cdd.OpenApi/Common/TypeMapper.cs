// <copyright file="TypeMapper.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Linq;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Common
{
    /// <summary>
    /// Canonical OpenAPI 3.2.0 and JSON Schema 2020-12 data type constants.
    /// </summary>
    public static class OpenApiDataType
    {
        /// <summary>The <c>integer</c> data type.</summary>
        public const string Integer = "integer";

        /// <summary>The <c>number</c> data type.</summary>
        public const string Number = "number";

        /// <summary>The <c>boolean</c> data type.</summary>
        public const string Boolean = "boolean";

        /// <summary>The <c>string</c> data type.</summary>
        public const string String = "string";

        /// <summary>The <c>array</c> data type.</summary>
        public const string Array = "array";

        /// <summary>The <c>object</c> data type.</summary>
        public const string Object = "object";
    }

    /// <summary>
    /// Canonical OpenAPI 3.2.0 data format modifier constants.
    /// </summary>
    public static class OpenApiDataFormat
    {
        /// <summary>Signed 32-bit integer format (<c>int32</c>).</summary>
        public const string Int32 = "int32";

        /// <summary>Signed 64-bit integer format (<c>int64</c>).</summary>
        public const string Int64 = "int64";

        /// <summary>Single-precision floating-point format (<c>float</c>).</summary>
        public const string Float = "float";

        /// <summary>Double-precision floating-point format (<c>double</c>).</summary>
        public const string Double = "double";

        /// <summary>High-precision decimal format (<c>decimal</c>).</summary>
        public const string Decimal = "decimal";

        /// <summary>UUID / GUID format (<c>uuid</c>).</summary>
        public const string Uuid = "uuid";

        /// <summary>RFC 3339 date-time format (<c>date-time</c>).</summary>
        public const string DateTime = "date-time";

        /// <summary>RFC 3339 full-date format (<c>date</c>).</summary>
        public const string Date = "date";

        /// <summary>RFC 3339 full-time format (<c>time</c>).</summary>
        public const string Time = "time";

        /// <summary>RFC 3986 URI format (<c>uri</c>).</summary>
        public const string Uri = "uri";

        /// <summary>Base64-encoded characters format (<c>byte</c>).</summary>
        public const string Byte = "byte";

        /// <summary>Raw binary octets format (<c>binary</c>).</summary>
        public const string Binary = "binary";
    }

    /// <summary>
    /// Provides unified bidirectional type mapping between OpenAPI schema types/formats and C# type names.
    /// </summary>
    public static class TypeMapper
    {
        /// <summary>
        /// Maps an OpenAPI primitive type string to its corresponding C# type name.
        /// </summary>
        /// <param name="openApiType">The OpenAPI type string (e.g., "integer", "number", "boolean", "string").</param>
        /// <param name="fallbackType">The fallback C# type name when the OpenAPI type is unrecognized or null.</param>
        /// <returns>The mapped C# type name.</returns>
        public static string MapOpenApiToCSharp(string? openApiType, string fallbackType = "string")
        {
            return openApiType switch
            {
                OpenApiDataType.Integer => "int",
                OpenApiDataType.Number => "double",
                OpenApiDataType.Boolean => "bool",
                OpenApiDataType.String => "string",
                _ => fallbackType
            };
        }

        /// <summary>
        /// Maps a full <see cref="OpenApiSchema"/> (including <c>format</c>, <c>$ref</c>, <c>items</c>, and <c>additionalProperties</c>) to its C# type representation.
        /// </summary>
        /// <param name="schema">The OpenAPI schema to resolve.</param>
        /// <param name="fallbackType">The fallback C# type name if the schema is null or untyped.</param>
        /// <returns>The resolved C# type name.</returns>
        public static string MapSchemaToCSharp(OpenApiSchema? schema, string fallbackType = "object")
        {
            if (schema == null) return fallbackType;

            if (!string.IsNullOrEmpty(schema.Ref))
            {
                return schema.Ref.Split('/').Last();
            }

            if (schema.Type == OpenApiDataType.Array)
            {
                var itemType = MapSchemaToCSharp(schema.Items, "object");
                return $"List<{itemType}>";
            }

            if (schema.Type == OpenApiDataType.Object && schema.AdditionalProperties != null)
            {
                var valType = MapSchemaToCSharp(schema.AdditionalProperties.Schema, "object");
                return $"Dictionary<string, {valType}>";
            }

            if (schema.Type == OpenApiDataType.Integer)
            {
                return schema.Format switch
                {
                    OpenApiDataFormat.Int64 => "long",
                    _ => "int"
                };
            }

            if (schema.Type == OpenApiDataType.Number)
            {
                return schema.Format switch
                {
                    OpenApiDataFormat.Float => "float",
                    OpenApiDataFormat.Decimal => "decimal",
                    _ => "double"
                };
            }

            if (schema.Type == OpenApiDataType.String)
            {
                return schema.Format switch
                {
                    OpenApiDataFormat.Uuid => "Guid",
                    OpenApiDataFormat.DateTime => "DateTimeOffset",
                    OpenApiDataFormat.Date => "DateOnly",
                    OpenApiDataFormat.Time => "TimeOnly",
                    OpenApiDataFormat.Uri => "Uri",
                    OpenApiDataFormat.Byte or OpenApiDataFormat.Binary => "byte[]",
                    _ => "string"
                };
            }

            return MapOpenApiToCSharp(schema.Type, fallbackType);
        }

        /// <summary>
        /// Maps a C# primitive or model type name to its corresponding OpenAPI type string.
        /// </summary>
        /// <param name="csharpType">The C# type name (e.g., "int", "double", "bool", "string").</param>
        /// <param name="fallbackType">The fallback OpenAPI type string when the C# type is unrecognized.</param>
        /// <returns>The mapped OpenAPI type string.</returns>
        public static string MapCSharpToOpenApi(string? csharpType, string fallbackType = "string")
        {
            return csharpType switch
            {
                "int" or "long" or "short" or "Int32" or "Int64" or "Int16" => OpenApiDataType.Integer,
                "float" or "double" or "decimal" or "Single" or "Double" or "Decimal" => OpenApiDataType.Number,
                "bool" or "Boolean" => OpenApiDataType.Boolean,
                "string" or "String" or "Guid" or "System.Guid" or "DateTime" or "System.DateTime" or "DateTimeOffset" or "System.DateTimeOffset" or "DateOnly" or "System.DateOnly" or "TimeOnly" or "System.TimeOnly" or "Uri" or "System.Uri" or "byte[]" => OpenApiDataType.String,
                _ => fallbackType
            };
        }

        /// <summary>
        /// Maps a C# type name to a tuple of OpenAPI <c>(Type, Format)</c>.
        /// </summary>
        /// <param name="csharpType">The C# type name.</param>
        /// <param name="fallbackType">The fallback OpenAPI type string when unrecognized.</param>
        /// <returns>A tuple containing the OpenAPI <c>type</c> and optional <c>format</c>.</returns>
        public static (string Type, string? Format) MapCSharpToOpenApiTypeAndFormat(string? csharpType, string fallbackType = "string")
        {
            return csharpType switch
            {
                "int" or "Int32" or "short" or "Int16" => (OpenApiDataType.Integer, null),
                "long" or "Int64" => (OpenApiDataType.Integer, OpenApiDataFormat.Int64),
                "float" or "Single" => (OpenApiDataType.Number, OpenApiDataFormat.Float),
                "decimal" or "Decimal" => (OpenApiDataType.Number, OpenApiDataFormat.Decimal),
                "double" or "Double" => (OpenApiDataType.Number, null),
                "bool" or "Boolean" => (OpenApiDataType.Boolean, null),
                "Guid" or "System.Guid" => (OpenApiDataType.String, OpenApiDataFormat.Uuid),
                "DateTime" or "System.DateTime" or "DateTimeOffset" or "System.DateTimeOffset" => (OpenApiDataType.String, OpenApiDataFormat.DateTime),
                "DateOnly" or "System.DateOnly" => (OpenApiDataType.String, OpenApiDataFormat.Date),
                "TimeOnly" or "System.TimeOnly" => (OpenApiDataType.String, OpenApiDataFormat.Time),
                "Uri" or "System.Uri" => (OpenApiDataType.String, OpenApiDataFormat.Uri),
                "byte[]" => (OpenApiDataType.String, OpenApiDataFormat.Byte),
                "string" or "String" => (OpenApiDataType.String, null),
                _ => (fallbackType, null)
            };
        }
    }
}
