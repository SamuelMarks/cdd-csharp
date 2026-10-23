// <copyright file="parse.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.CliModule
{
    /// <summary>Parses C# command-line interface (CLI) classes into <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and <see cref="OpenApiPaths"/> definitions.</summary>
    public static class Parse
    {
        /// <summary>
        /// Extracts a list of <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances and path metadata by analyzing command switch statements in a C# CLI class.
        /// </summary>
        /// <param name="classNode">The C# CLI class syntax node to analyze.</param>
        /// <returns>A tuple of extracted <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances and path metadata.</returns>
        public static (List<Cdd.OpenApi.Ir.IrEndpoint> Endpoints, Dictionary<string, Cdd.OpenApi.Ir.IrPathMetadata> PathMetadata) ToEndpoints(ClassDeclarationSyntax classNode)
        {
            var paths = ToPaths(classNode);
            var metadata = new Dictionary<string, Cdd.OpenApi.Ir.IrPathMetadata>();
            foreach (var kvp in paths)
            {
                metadata[kvp.Key] = new Cdd.OpenApi.Ir.IrPathMetadata
                {
                    Parameters = kvp.Value.Parameters
                };
            }

            return (Cdd.OpenApi.Common.OperationHelpers.ExtractEndpointsFromPaths(paths), metadata);
        }

        /// <summary>
        /// Extracts an <see cref="OpenApiPaths"/> object by analyzing command switch statements in a C# CLI class.
        /// </summary>
        /// <param name="classNode">The C# CLI class syntax node to analyze.</param>
        /// <returns>The extracted <see cref="OpenApiPaths"/>.</returns>
        public static OpenApiPaths ToPaths(ClassDeclarationSyntax classNode)
        {
            var paths = new OpenApiPaths();

            var switchStmts = classNode.GetDescendantNodesSafe().OfType<SwitchStatementSyntax>();
            foreach (var switchStmt in switchStmts)
            {
                foreach (var section in switchStmt.Sections)
                {
                    foreach (var label in section.Labels.OfType<CaseSwitchLabelSyntax>())
                    {
                        if (label.Value is LiteralExpressionSyntax literal && literal.Token.IsKind(SyntaxKind.StringLiteralToken))
                        {
                            var commandName = literal.Token.ValueText;
                            if (commandName == "mcp")
                            {
                                paths["/mcp"] = new OpenApiPathItem
                                {
                                    Post = new OpenApiOperation
                                    {
                                        OperationId = "mcp",
                                        Summary = "Start the MCP stdio server",
                                        Responses = new OpenApiResponses
                                        {
                                            ["200"] = new OpenApiResponse { Description = "MCP Server started" }
                                        }
                                    }
                                };
                                continue;
                            }

                            var parameters = new List<OpenApiParameter>();
                            var localDecls = section.Statements.OfType<LocalDeclarationStatementSyntax>();
                            foreach (var decl in localDecls)
                            {
                                var type = decl.Declaration.Type.ToString();
                                var trailingTrivia = decl.GetTrailingTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia)).ToString().TrimStart('/', ' ').Trim();
                                var leadingTrivia = decl.GetLeadingTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia)).ToString().TrimStart('/', ' ').Trim();
                                var desc = string.IsNullOrEmpty(trailingTrivia) ? leadingTrivia : trailingTrivia;
                                if (string.IsNullOrEmpty(desc)) desc = null;

                                foreach (var variable in decl.Declaration.Variables)
                                {
                                    var name = variable.Identifier.Text;
                                    var paramName = name.Replace("_", "-");
                                    var initializer = variable.Initializer?.Value;
                                    OpenApiAny? example = null;
                                    if (initializer is LiteralExpressionSyntax lit)
                                    {
                                        if (lit.Token.IsKind(SyntaxKind.NumericLiteralToken))
                                            example = new OpenApiAny(lit.Token.Text, System.Text.Json.JsonValueKind.Number);
                                        else if (lit.Token.IsKind(SyntaxKind.StringLiteralToken))
                                            example = lit.Token.ValueText;
                                    }

                                    parameters.Add(new OpenApiParameter
                                    {
                                        Name = paramName,
                                        In = "query",
                                        Description = desc,
                                        Example = example,
                                        Schema = new OpenApiSchema { Type = type == "int" ? "integer" : "string" }
                                    });
                                }
                            }

                            var operation = new OpenApiOperation
                            {
                                OperationId = commandName,
                                Summary = "Command " + commandName
                            };

                            var pathItem = new OpenApiPathItem();

                            switch (commandName.ToLower())
                            {
                                case "put": pathItem.Put = operation; break;
                                case "post": pathItem.Post = operation; break;
                                case "delete": pathItem.Delete = operation; break;
                                case "options": pathItem.Options = operation; break;
                                case "head": pathItem.Head = operation; break;
                                case "patch": pathItem.Patch = operation; break;
                                case "trace": pathItem.Trace = operation; break;
                                case "query": pathItem.Query = operation; break;
                                default: pathItem.Get = operation; break;
                            }

                            if (parameters.Any())
                            {
                                pathItem.Parameters = parameters;
                            }

                            paths["/" + commandName] = pathItem;
                        }
                    }
                }
            }

            return paths;
        }
    }
}
