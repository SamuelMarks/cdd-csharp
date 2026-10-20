// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Routes
{
    /// <summary>Emits ASP.NET Core route controller interfaces from <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and <see cref="OpenApiPaths"/> definitions.</summary>
    public static class Emit
    {
        /// <summary>
        /// Generates an ASP.NET Core controller route interface from a list of <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> instances.
        /// </summary>
        /// <param name="interfaceName">The name of the generated interface.</param>
        /// <param name="endpoints">The IR endpoints to convert into route methods.</param>
        /// <param name="pathMetadata">Optional path-level metadata keyed by path template.</param>
        /// <param name="includeMcp">Whether to include MCP SSE and JSON-RPC message endpoints.</param>
        /// <returns>The generated <see cref="InterfaceDeclarationSyntax"/>.</returns>
        public static InterfaceDeclarationSyntax ToInterface(
            string interfaceName,
            IReadOnlyList<Cdd.OpenApi.Ir.IrEndpoint> endpoints,
            IReadOnlyDictionary<string, Cdd.OpenApi.Ir.IrPathMetadata>? pathMetadata = null,
            bool includeMcp = true)
        {
            return ToInterface(interfaceName, OperationHelpers.ConvertEndpointsToPaths(endpoints, pathMetadata), includeMcp);
        }

        /// <summary>
        /// Generates an ASP.NET Core controller route interface from an <see cref="OpenApiPaths"/> collection.
        /// </summary>
        /// <param name="interfaceName">The name of the generated interface.</param>
        /// <param name="paths">The OpenAPI paths to convert into route methods.</param>
        /// <param name="includeMcp">Whether to include MCP SSE and JSON-RPC message endpoints.</param>
        /// <returns>The generated <see cref="InterfaceDeclarationSyntax"/>.</returns>
        public static InterfaceDeclarationSyntax ToInterface(string interfaceName, OpenApiPaths paths, bool includeMcp = true)
        {
            var interfaceNode = SyntaxFactory.InterfaceDeclaration(interfaceName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var members = new List<MemberDeclarationSyntax>();
            foreach (var pathKvp in paths)
            {
                var routePath = pathKvp.Key;
                var pathItem = pathKvp.Value;

                var operations = OperationHelpers.GetOperationsByPascalVerb(pathItem);

                foreach (var opKvp in operations)
                {
                    var httpMethod = opKvp.Key;
                    var operation = opKvp.Value;
                    if (operation == null) continue;

                    var methodName = operation.OperationId ?? $"{httpMethod}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                    var returnTypeName = "void";
                    if (operation.Responses != null && operation.Responses.Any())
                    {
                        returnTypeName = "System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>";
                        var successResponse = operation.Responses.FirstOrDefault(r => r.Key.StartsWith("2"));
                        if (successResponse.Value != null && successResponse.Value.Content != null && successResponse.Value.Content.TryGetValue("application/json", out var mediaType))
                        {
                            var schemaType = mediaType.Schema?.Type;
                            if (schemaType != null)
                            {
                                returnTypeName = $"System.Threading.Tasks.Task<{TypeMapper.MapOpenApiToCSharp(schemaType, "string")}>";
                            }
                        }
                    }

                    var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnTypeName), methodName)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    var attrList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName($"Http{httpMethod}"))
                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(routePath))))))));

                    methodNode = methodNode.AddAttributeLists(attrList);
                    var methodParams = new List<ParameterSyntax>();

                    if (operation.Parameters != null)
                    {
                        var parameters = operation.Parameters.Select(p =>
                        {
                            var param = SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(TypeMapper.MapOpenApiToCSharp(p.Schema?.Type, "string")));

                            if (p.In == "query")
                            {
                                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromQuery")))));
                            }
                            else if (p.In == "path")
                            {
                                param = param.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromRoute")))));
                            }

                            return OperationSignatureBuilder.ApplyParameterMetadata(param, p);
                        }).ToList();

                        methodParams.AddRange(parameters);
                    }

                    if (operation.RequestBody != null)
                    {
                        var mediaType = operation.RequestBody.Content.Values.FirstOrDefault();
                        var schemaType = mediaType?.Schema?.Type;

                        var typeName = TypeMapper.MapOpenApiToCSharp(schemaType, "string");
                        if (typeName == "string") typeName = "object";

                        var param = SyntaxFactory.Parameter(SyntaxFactory.Identifier("body"))
                            .WithType(SyntaxFactory.ParseTypeName(typeName))
                            .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromBody")))));
                        methodParams.Add(param);
                    }

                    if (methodParams.Any())
                    {
                        methodNode = methodNode.AddParameterListParameters(methodParams.ToArray());
                    }

                    methodNode = OperationSignatureBuilder.ApplyOperationDocstrings(methodNode, operation, includeParamDocs: false);
                    members.Add(methodNode);
                }
            }

            if (includeMcp)
            {
                var sseMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>"), "McpSseEndpoint")
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HttpGet"))
                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("/mcp/sse")))))))));
                sseMethod = Docstrings.Emit.WithSummary(sseMethod, "Establishes a Model Context Protocol SSE stream.");
                members.Add(sseMethod);

                var msgMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult>"), "McpMessageEndpoint")
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                    .AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("HttpPost"))
                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("/mcp/messages")))))))))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("body")).WithType(SyntaxFactory.ParseTypeName("object")).AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromBody"))))))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("sessionId")).WithType(SyntaxFactory.ParseTypeName("string")).AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("FromQuery"))))));
                msgMethod = Docstrings.Emit.WithSummary(msgMethod, "Receives JSON-RPC messages for an active MCP session.");
                members.Add(msgMethod);
            }

            return interfaceNode.AddMembers(members.ToArray());
        }
    }
}
