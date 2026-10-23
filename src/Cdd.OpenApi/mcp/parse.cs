// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Mcp
{
    /// <summary>
    /// Parses C# Model Context Protocol (MCP) server classes and protocol models into <see cref="IrEndpoint"/>, <see cref="OpenApiPaths"/>, and <see cref="OpenApiSchema"/> definitions.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Determines whether the specified <see cref="ClassDeclarationSyntax"/> represents an MCP server class.
        /// </summary>
        /// <param name="classNode">The C# class declaration syntax node to inspect.</param>
        /// <returns><c>true</c> if the class is an MCP server class; otherwise <c>false</c>.</returns>
        public static bool IsMcpServerClass(ClassDeclarationSyntax classNode)
        {
            if (classNode == null)
            {
                return false;
            }

            if (classNode.Identifier.Text.EndsWith("McpServer", StringComparison.Ordinal))
            {
                return true;
            }

            return classNode.GetDescendantNodesSafe()
                .OfType<LiteralExpressionSyntax>()
                .Any(l => l.Token.ValueText.Contains("tools/list", StringComparison.Ordinal) ||
                          l.Token.ValueText.Contains("tools/call", StringComparison.Ordinal));
        }

        /// <summary>
        /// Extracts a list of <see cref="IrEndpoint"/> instances from a generated C# MCP server class declaration.
        /// </summary>
        /// <param name="classNode">The C# MCP server class syntax node to analyze.</param>
        /// <returns>A list of extracted <see cref="IrEndpoint"/> instances.</returns>
        public static List<IrEndpoint> ToEndpoints(ClassDeclarationSyntax classNode)
        {
            return OperationHelpers.ExtractEndpointsFromPaths(ToPaths(classNode));
        }

        /// <summary>
        /// Extracts an <see cref="OpenApiPaths"/> collection from a generated C# MCP server class declaration by inspecting its embedded <c>tools/list</c> JSON metadata and <c>tools/call</c> dispatch statements.
        /// </summary>
        /// <param name="classNode">The C# MCP server class syntax node to analyze.</param>
        /// <returns>The extracted <see cref="OpenApiPaths"/>.</returns>
        public static OpenApiPaths ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new OpenApiPaths();
            if (classNode == null)
            {
                return paths;
            }

            var literals = classNode.GetDescendantNodesSafe()
                .OfType<LiteralExpressionSyntax>()
                .Select(l => l.Token.ValueText)
                .ToList();

            foreach (var lit in literals)
            {
                if (!lit.Contains("inputSchema", StringComparison.Ordinal))
                {
                    continue;
                }

                TryExtractToolsFromRawJson(lit, paths);
            }

            return paths;
        }

        /// <summary>
        /// Parses a serialized JSON tools array fragment into <see cref="OpenApiPaths"/> entries.
        /// </summary>
        /// <param name="rawJson">The raw JSON fragment containing MCP tool definitions.</param>
        /// <param name="paths">The target <see cref="OpenApiPaths"/> dictionary to populate.</param>
        public static void TryExtractToolsFromRawJson(string rawJson, OpenApiPaths paths)
        {
            if (string.IsNullOrWhiteSpace(rawJson) || paths == null)
            {
                return;
            }

            var wrapped = rawJson.TrimStart().StartsWith("[", StringComparison.Ordinal)
                ? rawJson
                : "[" + rawJson + "]";

            using var doc = JsonDocument.Parse(wrapped);
            foreach (var toolElem in doc.RootElement.EnumerateArray())
            {
                if (!toolElem.TryGetProperty("name", out var nameProp))
                {
                    continue;
                }

                var toolName = nameProp.GetString();
                if (string.IsNullOrEmpty(toolName))
                {
                    continue;
                }

                var description = toolElem.TryGetProperty("description", out var descProp)
                    ? descProp.GetString()
                    : null;

                var parameters = new List<OpenApiParameter>();
                var requiredSet = new HashSet<string>(StringComparer.Ordinal);
                var schemaElem = toolElem.GetProperty("inputSchema");

                if (schemaElem.TryGetProperty("required", out var reqElem))
                {
                    foreach (var r in reqElem.EnumerateArray())
                    {
                        requiredSet.Add(r.GetString()!);
                    }
                }

                if (schemaElem.TryGetProperty("properties", out var propsElem))
                {
                    foreach (var prop in propsElem.EnumerateObject())
                    {
                        var pType = prop.Value.GetProperty("type").GetString()!;
                        var pDesc = prop.Value.GetProperty("description").GetString();
                        parameters.Add(new OpenApiParameter
                        {
                            Name = prop.Name,
                            In = "query",
                            Required = requiredSet.Contains(prop.Name),
                            Description = string.IsNullOrEmpty(pDesc) ? null : pDesc,
                            Schema = new OpenApiSchema { Type = pType == "number" ? "integer" : pType }
                        });
                    }
                }

                var routePath = "/" + Emit.ToSnakeCase(toolName).TrimStart('/');
                var op = new OpenApiOperation
                {
                    OperationId = toolName,
                    Summary = description,
                    Parameters = parameters.Count > 0 ? parameters : null,
                    Responses = new OpenApiResponses
                    {
                        ["200"] = new OpenApiResponse { Description = "Success" }
                    }
                };

                paths[routePath] = new OpenApiPathItem { Post = op };
            }
        }
    }
}
