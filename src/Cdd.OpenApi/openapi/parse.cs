// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.Json;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Parse
{
    /// <summary>Parses OpenAPI 3.x and Swagger 2.0 JSON or YAML strings into <see cref="OpenApiDocument"/> models.</summary>
    public class OpenApiParser
    {
        /// <summary>Initializes a new instance of the <see cref="OpenApiParser"/> class.</summary>
        public OpenApiParser()
        {
        }

        /// <summary>
        /// Parses an OpenAPI or Swagger JSON or YAML string into a normalized <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="content">The raw JSON or YAML string to parse.</param>
        /// <returns>The deserialized and normalized <see cref="OpenApiDocument"/>.</returns>
        public OpenApiDocument ParseYamlOrJson(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("OpenAPI content cannot be null or empty.", nameof(content));
            }

            var trimmed = content.TrimStart();
            if (trimmed.StartsWith("{", StringComparison.Ordinal) || string.Equals(trimmed, "null", StringComparison.Ordinal))
            {
                return ParseJson(content);
            }

            var jsonNode = YamlConverter.ConvertYamlToJsonNode(content);
            return ParseJson(jsonNode.ToJsonString());
        }

        /// <summary>
        /// Parses an OpenAPI or Swagger JSON string into a normalized <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="jsonContent">The raw JSON string to parse.</param>
        /// <returns>The deserialized and normalized <see cref="OpenApiDocument"/>.</returns>
        public OpenApiDocument ParseJson(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException("JSON content cannot be null or empty.", nameof(jsonContent));
            }

            try
            {
                var document = JsonSerializer.Deserialize(jsonContent, OpenApiJsonContext.Default.OpenApiDocument);
                if (document != null)
                {
                    ConvertSwagger2Definitions(document);
                    ConvertSwagger2Paths(document);
                }

                return document ?? new OpenApiDocument();
            }
            catch (JsonException ex)
            {
                throw new FormatException($"Failed to parse OpenAPI JSON: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Converts Swagger 2.0 <c>definitions</c> into OpenAPI 3.x <c>components.schemas</c>.
        /// </summary>
        /// <param name="document">The document to normalize.</param>
        private static void ConvertSwagger2Definitions(OpenApiDocument document)
        {
            if (document.Definitions == null)
            {
                return;
            }

            document.Components ??= new OpenApiComponents();
            document.Components.Schemas ??= new Dictionary<string, OpenApiSchema>();
            foreach (var def in document.Definitions)
            {
                if (!document.Components.Schemas.ContainsKey(def.Key))
                {
                    document.Components.Schemas[def.Key] = def.Value;
                }
            }
        }

        /// <summary>
        /// Converts Swagger 2.0 path operations (<c>in: body</c>, <c>in: formData</c>, top-level parameter <c>type</c>, and response <c>schema</c>) into OpenAPI 3.x structures.
        /// </summary>
        /// <param name="document">The document to normalize.</param>
        private static void ConvertSwagger2Paths(OpenApiDocument document)
        {
            if (document.Paths == null)
            {
                return;
            }

            foreach (var path in document.Paths.Values)
            {
                var ops = new[] { path.Get, path.Put, path.Post, path.Delete, path.Options, path.Head, path.Patch, path.Trace };
                foreach (var op in ops)
                {
                    if (op == null) continue;
                    ConvertSwagger2OperationParameters(op);
                    ConvertSwagger2OperationResponses(op);
                }
            }
        }

        /// <summary>
        /// Converts Swagger 2.0 <c>body</c>, <c>formData</c>, and primitive parameters on an <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="op">The operation to normalize.</param>
        private static void ConvertSwagger2OperationParameters(OpenApiOperation op)
        {
            if (op.Parameters == null)
            {
                return;
            }

            var newParams = new List<OpenApiParameter>();
            foreach (var p in op.Parameters)
            {
                if (p.In == "body")
                {
                    if (p.Schema != null)
                    {
                        op.RequestBody = new OpenApiRequestBody
                        {
                            Description = p.Description ?? string.Empty,
                            Required = p.Required,
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new OpenApiMediaType { Schema = p.Schema }
                            }
                        };
                    }
                    else
                    {
                        newParams.Add(p);
                    }
                }
                else if (p.In == "formData")
                {
                    if (op.RequestBody == null)
                    {
                        op.RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/x-www-form-urlencoded"] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema { Type = OpenApiDataType.Object, Properties = new Dictionary<string, OpenApiSchema>() }
                                }
                            }
                        };
                    }

                    var formSchema = op.RequestBody.Content["application/x-www-form-urlencoded"].Schema;
                    if (formSchema != null && formSchema.Properties != null)
                    {
                        formSchema.Properties[p.Name] = p.Schema ?? new OpenApiSchema { Type = p.Type, Format = p.Format };
                    }
                }
                else
                {
                    if (p.Schema == null && p.Type != null)
                    {
                        p.Schema = new OpenApiSchema { Type = p.Type, Format = p.Format, Items = p.Items };
                    }

                    newParams.Add(p);
                }
            }

            op.Parameters = newParams.Count > 0 ? newParams : null;
        }

        /// <summary>
        /// Converts Swagger 2.0 response-level <c>schema</c> into OpenAPI 3.x <c>content["application/json"].schema</c>.
        /// </summary>
        /// <param name="op">The operation whose responses should be normalized.</param>
        private static void ConvertSwagger2OperationResponses(OpenApiOperation op)
        {
            if (op.Responses == null)
            {
                return;
            }

            foreach (var resp in op.Responses.Values)
            {
                if (resp.Schema != null && resp.Content == null)
                {
                    resp.Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType { Schema = resp.Schema }
                    };
                }
            }
        }
    }
}
