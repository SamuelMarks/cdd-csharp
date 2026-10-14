// <copyright file="TypeMapper.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Linq;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Common
{
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
                "integer" => "int",
                "number" => "double",
                "boolean" => "bool",
                "string" => "string",
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

            if (schema.Type == "array")
            {
                var itemType = MapSchemaToCSharp(schema.Items, "object");
                return $"List<{itemType}>";
            }

            if (schema.Type == "object" && schema.AdditionalProperties != null)
            {
                var valType = MapSchemaToCSharp(schema.AdditionalProperties.Schema, "object");
                return $"Dictionary<string, {valType}>";
            }

            if (schema.Type == "integer")
            {
                return schema.Format switch
                {
                    "int64" => "long",
                    _ => "int"
                };
            }

            if (schema.Type == "number")
            {
                return schema.Format switch
                {
                    "float" => "float",
                    "decimal" => "decimal",
                    _ => "double"
                };
            }

            if (schema.Type == "string")
            {
                return schema.Format switch
                {
                    "uuid" => "Guid",
                    "date-time" => "DateTimeOffset",
                    "date" => "DateOnly",
                    "time" => "TimeOnly",
                    "uri" => "Uri",
                    "byte" or "binary" => "byte[]",
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
                "int" or "long" or "short" => "integer",
                "float" or "double" or "decimal" => "number",
                "bool" => "boolean",
                "string" => "string",
                _ => fallbackType
            };
        }
    }
}
