using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Routes
{
/// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
/// <summary>Auto-generated documentation for ToInterface.</summary>
        public static InterfaceDeclarationSyntax ToInterface(string interfaceName, OpenApiPaths paths)
        {
            var interfaceNode = SyntaxFactory.InterfaceDeclaration(interfaceName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            foreach (var pathKvp in paths)
            {
                var routePath = pathKvp.Key;
                var pathItem = pathKvp.Value;

                var operations = new Dictionary<string, OpenApiOperation?>
                {
                    { "Get", pathItem.Get },
                    { "Put", pathItem.Put },
                    { "Post", pathItem.Post },
                    { "Delete", pathItem.Delete },
                    { "Options", pathItem.Options },
                    { "Head", pathItem.Head },
                    { "Patch", pathItem.Patch },
                    { "Trace", pathItem.Trace }
                };

                foreach (var opKvp in operations)
                {
                    var httpMethod = opKvp.Key;
                    var operation = opKvp.Value;
                    if (operation == null) continue;

                    var methodName = operation.OperationId ?? $"{httpMethod}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}";

                    var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), methodName)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

                    // Add Route Attribute e.g. [HttpGet("/pets")]
                    var attrList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName($"Http{httpMethod}"))
                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.AttributeArgument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(routePath))
                                    )
                                )
                            ))
                    ));

                    methodNode = methodNode.AddAttributeLists(attrList);

                    if (operation.Parameters != null)
                    {
                        var parameters = operation.Parameters.Select(p => 
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(MapTypeToCSharp(p.Schema?.Type)))
                        ).ToArray();

                        methodNode = methodNode.AddParameterListParameters(parameters);
                    }

                    if (!string.IsNullOrWhiteSpace(operation.Summary))
                    {
                        methodNode = Docstrings.Emit.WithSummary(methodNode, operation.Summary);
                    }

                    interfaceNode = interfaceNode.AddMembers(methodNode);
                }
            }

            return interfaceNode.NormalizeWhitespace();
        }

        private static string MapTypeToCSharp(string? openApiType)
        {
            return openApiType switch
            {
                "integer" => "int",
                "number" => "double",
                "boolean" => "bool",
                "string" => "string",
                _ => "string"
            };
        }
    }
}
