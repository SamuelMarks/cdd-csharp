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
                    { "Trace", pathItem.Trace },
                    { "Query", pathItem.Query }
                };

                if (pathItem.AdditionalOperations != null)
                {
                    foreach (var addOp in pathItem.AdditionalOperations)
                    {
                        var normVerb = addOp.Key.Substring(0, 1).ToUpperInvariant() + addOp.Key.Substring(1).ToLowerInvariant();
                        operations[normVerb] = addOp.Value;
                    }
                }

                foreach (var opKvp in operations)
                {
                    var httpMethod = opKvp.Key;
                    var operation = opKvp.Value;
                    if (operation == null) continue;

                    var methodName = operation.OperationId ?? $"{httpMethod}{routePath.Replace("/", "").Replace("{", "").Replace("}", "")}Async";
                    if (!methodName.EndsWith("Async")) methodName += "Async";

                    var returnTypeName = "string";
                    if (operation.Responses != null && operation.Responses.Any())
                    {
                        var successResponse = operation.Responses.FirstOrDefault(r => r.Key.StartsWith("2"));
                        if (successResponse.Value?.Content?.TryGetValue("application/json", out var mediaType) == true)
                        {
                            var schema = mediaType.Schema;
                            if (schema != null && schema.Type != null)
                            {
                                returnTypeName = MapTypeToCSharp(schema.Type);
                            }
                        }
                    }

                    var methodNode = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName($"System.Threading.Tasks.Task<{returnTypeName}>"), methodName)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword));

                    var parameters = new List<ParameterSyntax>();
                    if (operation.Parameters != null)
                    {
                        foreach (var p in operation.Parameters)
                        {
                            var pNode = SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(MapTypeToCSharp(p.Schema?.Type)));

                            if (p.Deprecated == true)
                            {
                                var obsoleteAttr = SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("System.Obsolete"))
                                    )
                                );
                                pNode = pNode.AddAttributeLists(obsoleteAttr);
                            }

                            if (p.AllowEmptyValue == true)
                            {
                                var allowEmptyAttr = SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("AllowEmptyValue"))
                                    )
                                );
                                pNode = pNode.AddAttributeLists(allowEmptyAttr);
                            }

                            if (p.Example != null)
                            {
                                string exampleStr = p.Example.ToString() ?? "";
                                ExpressionSyntax defaultValue;
                                if (p.Schema?.Type == "string")
                                {
                                    defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(exampleStr));
                                }
                                else if (p.Schema?.Type == "boolean")
                                {
                                    defaultValue = SyntaxFactory.LiteralExpression(exampleStr.ToLower() == "true" ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
                                }
                                else
                                {
                                    if (int.TryParse(exampleStr, out int i)) defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(i));
                                    else if (double.TryParse(exampleStr, out double d)) defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(d));
                                    else defaultValue = SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
                                }

                                pNode = pNode.WithDefault(SyntaxFactory.EqualsValueClause(defaultValue));
                            }


                            if (p.Examples != null && p.Examples.Count > 0)
                            {
                                var exampleArgs = new List<AttributeArgumentSyntax>();
                                foreach (var ex in p.Examples)
                                {
                                    exampleArgs.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(ex.Key))));
                                    var exVal = ex.Value.Value?.ToString() ?? "";
                                    exampleArgs.Add(SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(exVal))));
                                }
                                pNode = pNode.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Examples"))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(exampleArgs)))
                                )));
                            }
                            if (p.Style != null)
                            {
                                pNode = pNode.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Style"))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(p.Style)))
                                        )))
                                )));
                            }
                            if (p.Explode == true)
                            {
                                pNode = pNode.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Explode"))
                                )));
                            }
                            if (p.AllowReserved == true)
                            {
                                pNode = pNode.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("AllowReserved"))
                                )));
                            }
                            if (p.Content != null && p.Content.Count > 0)
                            {
                                var firstContent = p.Content.First();
                                var mediaType = firstContent.Key;
                                var schemaType = firstContent.Value.Schema?.Type ?? "string";
                                pNode = pNode.AddAttributeLists(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Content"))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[]
                                        {
                                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(mediaType))),
                                            SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(schemaType)))
                                        })))
                                )));
                            }

                            parameters.Add(pNode);
                        }
                    }

                    if (operation.RequestBody != null)
                    {
                        var reqBody = operation.RequestBody;
                        if (reqBody.Content != null)
                        {
                            var contentDict = reqBody.Content;
                            if (contentDict.TryGetValue("application/json", out var mediaType))
                            {
                                var schema = mediaType.Schema;
                                if (schema != null && schema.Type != null)
                                {
                                    var typeName = MapTypeToCSharp(schema.Type);
                                    parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("body"))
                                        .WithType(SyntaxFactory.ParseTypeName(typeName)));
                                }
                            }
                        }
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

                    InvocationExpressionSyntax httpClientCall;
                    if (operation.RequestBody != null)
                    {
                        callMethod = $"{httpMethod}AsJsonAsync";
                        httpClientCall = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_httpClient"),
                                SyntaxFactory.IdentifierName(callMethod)
                            )
                        ).AddArgumentListArguments(
                            SyntaxFactory.Argument(routePathExpr),
                            SyntaxFactory.Argument(SyntaxFactory.IdentifierName("body"))
                        );
                    }
                    else
                    {
                        httpClientCall = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("_httpClient"),
                                SyntaxFactory.IdentifierName(callMethod)
                            )
                        ).AddArgumentListArguments(SyntaxFactory.Argument(routePathExpr));
                    }

                    var awaitHttpClientCall = SyntaxFactory.AwaitExpression(httpClientCall);

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

                    if (operation.Parameters != null)
                    {
                        foreach (var opParam in operation.Parameters)
                        {
                            if (!string.IsNullOrEmpty(opParam.Description))
                            {
                                methodNode = Docstrings.Emit.WithTag(methodNode, "param", new Dictionary<string, string> { { "name", opParam.Name } }, opParam.Description);
                            }
                        }
                    }

                    if (operation.Responses != null)
                    {
                        foreach (var respKvp in operation.Responses)
                        {
                            var attrs = new Dictionary<string, string> { { "code", respKvp.Key } };
                            if (respKvp.Value.Headers != null && respKvp.Value.Headers.Count > 0)
                            {
                                var headerKvp = respKvp.Value.Headers.First();
                                attrs["header"] = headerKvp.Key;
                                var h = headerKvp.Value;
                                if (!string.IsNullOrEmpty(h.Description)) attrs["header-description"] = h.Description;
                                if (h.Required.HasValue) attrs["header-required"] = h.Required.Value.ToString().ToLower();
                                if (h.Deprecated.HasValue) attrs["header-deprecated"] = h.Deprecated.Value.ToString().ToLower();
                                if (h.Example != null) attrs["header-example"] = h.Example.ToString() ?? "";
                                if (h.Examples != null && h.Examples.Count > 0)
                                {
                                    attrs["header-examples"] = string.Join(",", h.Examples.Select(e => $"{e.Key}:{e.Value.Value}"));
                                }
                                if (!string.IsNullOrEmpty(h.Style)) attrs["header-style"] = h.Style;
                                if (h.Explode.HasValue) attrs["header-explode"] = h.Explode.Value.ToString().ToLower();
                                if (h.Schema?.Type != null) attrs["header-schema"] = h.Schema.Type;
                                if (h.Content != null && h.Content.Count > 0)
                                {
                                    var c = h.Content.First();
                                    attrs["header-content"] = $"{c.Key}:{c.Value.Schema?.Type}";
                                }
                            }
                            methodNode = Docstrings.Emit.WithTag(methodNode, "response", attrs, respKvp.Value.Description ?? "Response");
                        }
                    }

                    if (operation.Servers != null)
                    {
                        foreach (var s in operation.Servers)
                        {
                            var attrs = new Dictionary<string, string> { { "url", s.Url } };
                            if (!string.IsNullOrEmpty(s.Description)) attrs["description"] = s.Description;
                            methodNode = Docstrings.Emit.WithTag(methodNode, "server", attrs, "");
                        }
                    }

                    if (operation.Callbacks != null)
                    {
                        foreach (var cKvp in operation.Callbacks)
                        {
                            var cName = cKvp.Key;
                            var cbObj = cKvp.Value;
                            if (cbObj.Count > 0)
                            {
                                var expKvp = cbObj.First();
                                var expression = expKvp.Key;
                                var opItem = expKvp.Value;
                                var desc = opItem.Post?.Description ?? opItem.Get?.Description ?? "Callback";
                                
                                var attrs = new Dictionary<string, string> { { "name", cName }, { "expression", expression } };
                                methodNode = Docstrings.Emit.WithTag(methodNode, "callback", attrs, desc);
                            }
                        }
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
