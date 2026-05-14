using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Parse
{
/// <summary>Auto-generated documentation for OpenApiParser.</summary>
    public class OpenApiParser
    {
/// <summary>Auto-generated documentation for OpenApiParser.</summary>
        public OpenApiParser()
        {
        }

/// <summary>Auto-generated documentation for ParseJson.</summary>
        public OpenApiDocument ParseJson(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException("JSON content cannot be null or empty.", nameof(jsonContent));
            }

            try
            {
                var document = JsonSerializer.Deserialize<OpenApiDocument>(jsonContent, FallbackOptions);
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
                                        if (p.In == "body" && p.Schema != null)
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
                                            if (p.Schema == null && p.Type != null)
                                            {
                                                p.Schema = new OpenApiSchema { Type = p.Type, Format = p.Format, Items = p.Items };
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

        private static readonly JsonSerializerOptions FallbackOptions = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }
}