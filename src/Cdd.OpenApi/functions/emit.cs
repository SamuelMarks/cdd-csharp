// <copyright file="emit.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Linq;
using Cdd.OpenApi.Common;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi.Functions
{
    /// <summary>
    /// Emits C# static function declarations from <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and OpenAPI operation definitions.
    /// </summary>
    public static class Emit
    {
        /// <summary>
        /// Generates a <see cref="MethodDeclarationSyntax"/> from an <see cref="Cdd.OpenApi.Ir.IrEndpoint"/>.
        /// </summary>
        /// <param name="endpoint">The IR endpoint to convert into a C# function.</param>
        /// <returns>The generated <see cref="MethodDeclarationSyntax"/>.</returns>
        public static MethodDeclarationSyntax EmitFunction(Cdd.OpenApi.Ir.IrEndpoint endpoint)
        {
            return EmitFunction(endpoint.ToOpenApiOperation());
        }

        /// <summary>
        /// Generates a <see cref="MethodDeclarationSyntax"/> from an <see cref="OpenApiOperation"/>.
        /// </summary>
        /// <param name="operation">The OpenAPI operation to convert into a C# function.</param>
        /// <returns>The generated <see cref="MethodDeclarationSyntax"/>.</returns>
        public static MethodDeclarationSyntax EmitFunction(OpenApiOperation operation)
        {
            string returnTypeName = "void";
            if (operation.Responses != null &&
                operation.Responses.TryGetValue("200", out var okResp) &&
                okResp.Content != null &&
                okResp.Content.TryGetValue("application/json", out var mediaType) &&
                mediaType.Schema != null)
            {
                returnTypeName = TypeMapper.MapSchemaToCSharp(mediaType.Schema, "object");
            }

            var returnType = SyntaxFactory.ParseTypeName(returnTypeName);
            var methodName = operation.OperationId ?? "DefaultFunction";

            var methodDecl = SyntaxFactory.MethodDeclaration(returnType, methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword));

            if (operation.Parameters != null)
            {
                var paramNodes = operation.Parameters.Select(p =>
                {
                    var pType = TypeMapper.MapSchemaToCSharp(p.Schema, TypeMapper.MapOpenApiToCSharp(p.Type, "string"));
                    return SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                        .WithType(SyntaxFactory.ParseTypeName(pType));
                }).ToArray();

                methodDecl = methodDecl.AddParameterListParameters(paramNodes);
            }

            if (!string.IsNullOrWhiteSpace(operation.Summary))
            {
                methodDecl = Docstrings.Emit.WithSummary(methodDecl, operation.Summary);
            }

            var body = SyntaxFactory.Block();
            return methodDecl.WithBody(body);
        }
    }
}
