// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Parse
{
    /// <summary>Parses OpenAPI 3.x and Swagger 2.0 JSON strings into <see cref="OpenApiDocument"/> models.</summary>
    public class OpenApiParser
    {
        /// <summary>Initializes a new instance of the <see cref="OpenApiParser"/> class.</summary>
        public OpenApiParser()
        {
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
                    if (document.Definitions != null)
                    {
                        document.Components ??= new OpenApiComponents();
                        document.Components.Schemas ??= new System.Collections.Generic.Dictionary<string, OpenApiSchema>();
                        foreach (var def in document.Definitions)
                        {
                            if (!document.Components.Schemas.ContainsKey(def.Key))
                            {
                                document.Components.Schemas[def.Key] = def.Value;
                            }
                        }
                    }

                    if (document.Paths != null)
                    {
                        foreach (var path in document.Paths.Values)
                        {
                            var ops = new[] { path.Get, path.Put, path.Post, path.Delete, path.Options, path.Head, path.Patch, path.Trace };
                            foreach (var op in ops)
                            {
                                if (op == null) continue;
                                if (op.Parameters != null)
                                {
                                    var newParams = new System.Collections.Generic.List<OpenApiParameter>();
                                    foreach (var p in op.Parameters)
                                    {
                                        if (p.In == "body")
                                        {
                                            if (p.Schema != null)
                                            {
                                                op.RequestBody = new OpenApiRequestBody
                                                {
                                                    Description = p.Description ?? "",
                                                    Required = p.Required,
                                                    Content = new System.Collections.Generic.Dictionary<string, OpenApiMediaType>
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
                                                    Content = new System.Collections.Generic.Dictionary<string, OpenApiMediaType>
                                                    {
                                                        ["application/x-www-form-urlencoded"] = new OpenApiMediaType
                                                        {
                                                            Schema = new OpenApiSchema { Type = "object", Properties = new System.Collections.Generic.Dictionary<string, OpenApiSchema>() }
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
                                            if (p.Schema == null)
                                            {
                                                if (p.Type != null)
                                                {
                                                    p.Schema = new OpenApiSchema { Type = p.Type, Format = p.Format, Items = p.Items };
                                                }
                                            }
                                            newParams.Add(p);
                                        }
                                    }
                                    op.Parameters = newParams.Count > 0 ? newParams : null;
                                }
                                if (op.Responses != null)
                                {
                                    foreach (var resp in op.Responses.Values)
                                    {
                                        if (resp.Schema != null && resp.Content == null)
                                        {
                                            resp.Content = new System.Collections.Generic.Dictionary<string, OpenApiMediaType>
                                            {
                                                ["application/json"] = new OpenApiMediaType { Schema = resp.Schema }
                                            };
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return document ?? new OpenApiDocument();
            }
            catch (JsonException ex)
            {
                throw new FormatException($"Failed to parse OpenAPI JSON: {ex.Message}", ex);
            }
        }

    }
}
