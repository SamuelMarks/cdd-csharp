using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Clients
{
/// <summary>Auto-generated documentation for Emit.</summary>
    public static class Emit
    {
/// <summary>Auto-generated documentation for ToClient.</summary>
        public static ClassDeclarationSyntax ToClient(string className, OpenApiPaths paths)
        {
            var classNode = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            var httpClientType = SyntaxFactory.ParseTypeName("System.Net.Http.HttpClient");
            
            var varDecl = SyntaxFactory.VariableDeclarator("_httpClient");
            var varDeclaration = SyntaxFactory.VariableDeclaration(httpClientType)
                .AddVariables(varDecl);

            var fieldNode = SyntaxFactory.FieldDeclaration(varDeclaration)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            var param = SyntaxFactory.Parameter(SyntaxFactory.Identifier("httpClient"))
                .WithType(httpClientType);

            var ctorBody = SyntaxFactory.Block(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName("_httpClient"),
                        SyntaxFactory.IdentifierName("httpClient")
                    )
                )
            );

            var ctorNode = SyntaxFactory.ConstructorDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(param)
                .WithBody(ctorBody);

            classNode = classNode.AddMembers(fieldNode, ctorNode);

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

                    var methodName = operation.OperationId ?? $"{httpMethod}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}Async";
                    if (!methodName.EndsWith("Async")) methodName += "Async";

                    var returnType = SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task<string>");
                    
                    var methodNode = SyntaxFactory.MethodDeclaration(returnType, methodName)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword));

                    var parameters = new List<ParameterSyntax>();
                    if (operation.Parameters != null)
                    {
                        parameters.AddRange(operation.Parameters.Select(p => 
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(MapTypeToCSharp(p.Schema?.Type)))
                        ));
                    }

                    if (parameters.Any())
                    {
                        methodNode = methodNode.AddParameterListParameters(parameters.ToArray());
                    }

                    string interpolatedPath = routePath;
                    ExpressionSyntax routePathExpr;
                    
                    if (interpolatedPath.Contains("{"))
                    {
                        routePathExpr = SyntaxFactory.ParseExpression($"$\"{interpolatedPath}\"");
                    }
                    else
                    {
                        routePathExpr = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(routePath));
                    }

                    var callMethod = $"{httpMethod}Async";

                    var awaitHttpClientCall = SyntaxFactory.AwaitExpression(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_httpClient"),
                                SyntaxFactory.IdentifierName(callMethod)
                            )
                        ).AddArgumentListArguments(SyntaxFactory.Argument(routePathExpr))
                    );

                    var statement1 = SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                        .AddVariables(
                            SyntaxFactory.VariableDeclarator("response")
                            .WithInitializer(SyntaxFactory.EqualsValueClause(awaitHttpClientCall))
                        )
                    );

                    var statement2 = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("response"),
                                SyntaxFactory.IdentifierName("EnsureSuccessStatusCode")
                            )
                        )
                    );

                    var readAsStringCall = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("response"),
                                SyntaxFactory.IdentifierName("Content")
                            ),
                            SyntaxFactory.IdentifierName("ReadAsStringAsync")
                        )
                    );

                    var statement3 = SyntaxFactory.ReturnStatement(
                        SyntaxFactory.AwaitExpression(readAsStringCall)
                    );

                    methodNode = methodNode.WithBody(SyntaxFactory.Block(statement1, statement2, statement3));

                    if (!string.IsNullOrWhiteSpace(operation.Summary))
                    {
                        methodNode = Docstrings.Emit.WithSummary(methodNode, operation.Summary);
                    }

                    classNode = classNode.AddMembers(methodNode);
                }
            }

            return classNode.NormalizeWhitespace();
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
