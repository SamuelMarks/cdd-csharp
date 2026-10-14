// <copyright file="DocsJsonGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.DocsJson
{
    /// <summary>Generates JSON documentation.</summary>
    public static class DocsJsonGenerator
    {
        /// <summary>
        /// Generates the documentation JSON string containing C# code snippets for all operations in the OpenAPI document.
        /// </summary>
        /// <param name="doc">The OpenAPI document to generate documentation snippets for.</param>
        /// <param name="noImports">Whether to omit the <c>imports</c> field from generated code objects.</param>
        /// <param name="noWrapping">Whether to omit the <c>wrapper_start</c> and <c>wrapper_end</c> fields from generated code objects.</param>
        /// <returns>The serialized documentation JSON string.</returns>
        public static string Generate(OpenApiDocument doc, bool noImports, bool noWrapping)
        {
            var output = new DocsJsonOutput
            {
                Language = "csharp",
                Operations = new List<DocsJsonOperation>()
            };

            if (doc.Paths != null)
            {
                foreach (var pathKvp in doc.Paths)
                {
                    var routePath = pathKvp.Key;
                    var pathItem = pathKvp.Value;

                    var operations = new Dictionary<string, OpenApiOperation?>
                    {
                        { "get", pathItem.Get },
                        { "put", pathItem.Put },
                        { "post", pathItem.Post },
                        { "delete", pathItem.Delete },
                        { "options", pathItem.Options },
                        { "head", pathItem.Head },
                        { "patch", pathItem.Patch },
                        { "trace", pathItem.Trace }
                    };

                    foreach (var opKvp in operations)
                    {
                        var method = opKvp.Key;
                        var operation = opKvp.Value;
                        if (operation == null) continue;

                        var operationId = operation.OperationId;
                        var methodName = operationId ?? $"{method.ToUpperInvariant()}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                        var opOut = new DocsJsonOperation
                        {
                            Method = method.ToUpperInvariant(),
                            Path = routePath,
                            OperationId = operation.OperationId,
                            Code = new DocsJsonCode()
                        };

                        if (!noImports)
                        {
                            var importsLines = new List<string>
                            {
                                "using System;",
                                "using System.Threading.Tasks;",
                                "using Generated.Api;",
                                "using Generated.Models;"
                            };
                            opOut.Code.Imports = string.Join("\n", importsLines);
                        }

                        if (!noWrapping)
                        {
                            var wrapStart = new List<string>
                            {
                                "public class Example",
                                "{",
                                "    public static async Task Main()",
                                "    {"
                            };
                            opOut.Code.WrapperStart = string.Join("\n", wrapStart);

                            var wrapEnd = new List<string>
                            {
                                "    }",
                                "}"
                            };
                            opOut.Code.WrapperEnd = string.Join("\n", wrapEnd);
                        }

                        var snippetLines = new List<string>();
                        string indent = noWrapping ? "" : "        ";

                        var tag = "Default";
                        if (operation.Tags != null && operation.Tags.Count > 0)
                        {
                            tag = operation.Tags[0];
                        }
                        if (tag.Length > 0)
                        {
                            tag = char.ToUpperInvariant(tag[0]) + tag.Substring(1);
                        }
                        var clientName = $"{tag}ApiClient";

                        snippetLines.Add($"{indent}// Initialize the API client");
                        snippetLines.Add($"{indent}var client = new {clientName}();");
                        snippetLines.Add("");

                        // Map parameters
                        var parameters = new List<string>();
                        if (operation.Parameters != null)
                        {
                            foreach (var param in operation.Parameters)
                            {
                                if (param == null) continue;
                                if (string.IsNullOrEmpty(param.Name)) continue;
                                var pName = param.Name;
                                var pType = MapTypeToCSharp(param.Schema?.Type);
                                var pValue = GetDefaultValueForType(pType);
                                snippetLines.Add($"{indent}{pType} {pName} = {pValue};");
                                parameters.Add(pName);
                            }
                        }

                        if (operation.RequestBody != null)
                        {
                            snippetLines.Add($"{indent}var requestBody = new object(); // TODO: Initialize request body");
                            parameters.Add("requestBody");
                        }

                        var paramsString = string.Join(", ", parameters);
                        snippetLines.Add($"{indent}await client.{methodName}Async({paramsString});");

                        opOut.Code.Snippet = string.Join("\n", snippetLines);

                        output.Operations.Add(opOut);
                    }
                }
            }

            var resultList = new List<DocsJsonOutput> { output };
            return JsonSerializer.Serialize(resultList, DocsJsonContext.Default.ListDocsJsonOutput);
        }

        /// <summary>
        /// Maps an OpenAPI primitive type name to its corresponding C# type string.
        /// </summary>
        /// <param name="openApiType">The OpenAPI type string.</param>
        /// <returns>The mapped C# type name.</returns>
        private static string MapTypeToCSharp(string? openApiType)
        {
            return Cdd.OpenApi.Common.TypeMapper.MapOpenApiToCSharp(openApiType, "object");
        }

        /// <summary>
        /// Returns a sample C# literal expression string for a given C# type name.
        /// </summary>
        /// <param name="csharpType">The C# type name.</param>
        /// <returns>A C# literal expression string representing a default sample value.</returns>
        private static string GetDefaultValueForType(string csharpType)
        {
            return csharpType switch
            {
                "int" => "0",
                "double" => "0.0",
                "bool" => "false",
                "string" => "\"example\"",
                _ => "null"
            };
        }
    }
}
