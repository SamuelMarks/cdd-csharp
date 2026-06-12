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
            var members = new List<MemberDeclarationSyntax>();

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
                    if (pathItem.AdditionalOperations.Any())
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
                    if (operation.Responses != null)
                        if (operation.Responses.Any())
                        {
                            var successResponse = operation.Responses.FirstOrDefault(r => r.Key.StartsWith("2"));
                            if (successResponse.Value != null && successResponse.Value.Content != null && successResponse.Value.Content.TryGetValue("application/json", out var mediaType))
                            {
                                var schema = mediaType.Schema;
                                if (schema != null)
                                {
                                    if (!string.IsNullOrEmpty(schema.Ref))
                                    {
                                        returnTypeName = schema.Ref.Split('/').Last();
                                    }
                                    else if (schema.Type == "array" && schema.Items?.Ref != null)
                                    {
                                        returnTypeName = $"System.Collections.Generic.List<{schema.Items.Ref.Split('/').Last()}>";
                                    }
                                    else if (schema.Type == "array" && schema.Items?.Type != null)
                                    {
                                        returnTypeName = $"System.Collections.Generic.List<{MapTypeToCSharp(schema.Items.Type)}>";
                                    }
                                    else if (schema.Type != null)
                                    {
                                        returnTypeName = MapTypeToCSharp(schema.Type);
                                    }
                                    else if (schema.Items?.Ref != null)
                                    {
                                        returnTypeName = $"System.Collections.Generic.List<{schema.Items.Ref.Split('/').Last()}>";
                                    }
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
                            string paramType = "string";
                            if (p.Schema != null)
                            {
                                if (!string.IsNullOrEmpty(p.Schema.Ref)) paramType = p.Schema.Ref.Split('/').Last();
                                else if (p.Schema.Type == "array" && p.Schema.Items?.Ref != null) paramType = $"System.Collections.Generic.List<{p.Schema.Items.Ref.Split('/').Last()}>";
                                else if (p.Schema.Type == "array" && p.Schema.Items?.Type != null) paramType = $"System.Collections.Generic.List<{MapTypeToCSharp(p.Schema.Items.Type)}>";
                                else if (p.Schema.Type != null) paramType = MapTypeToCSharp(p.Schema.Type);
                                else if (p.Schema.Items?.Ref != null) paramType = $"System.Collections.Generic.List<{p.Schema.Items.Ref.Split('/').Last()}>";
                            }

                            var pNode = SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(paramType));

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
                                string exampleStr = p.Example.ToString()!;
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


                            if (p.Examples != null)
                                if (p.Examples.Count > 0)
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
                            if (p.Explode == true && p.Schema?.Type != "array")
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
                            if (p.Content != null)
                                if (p.Content.Count > 0)
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
                                if (schema != null)
                                {
                                    string typeName = "string";
                                    if (!string.IsNullOrEmpty(schema.Ref))
                                    {
                                        typeName = schema.Ref.Split('/').Last();
                                    }
                                    else if (schema.Type == "array")
                                    {
                                        if (schema.Items != null)
                                        {
                                            if (schema.Items.Ref != null) typeName = $"System.Collections.Generic.List<{schema.Items.Ref.Split('/').Last()}>";
                                            else if (schema.Items.Type != null) typeName = $"System.Collections.Generic.List<{MapTypeToCSharp(schema.Items.Type)}>";
                                        }
                                    }
                                    else if (schema.Type != null)
                                    {
                                        typeName = MapTypeToCSharp(schema.Type);
                                    }
                                    else if (schema.Items != null && schema.Items.Ref != null)
                                    {
                                        typeName = $"System.Collections.Generic.List<{schema.Items.Ref.Split('/').Last()}>";
                                    }

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
                    if (interpolatedPath.StartsWith("/"))
                    {
                        interpolatedPath = interpolatedPath.Substring(1);
                    }

                    var pathParams = operation.Parameters?.Where(p => p.In == "path").ToList();
                    if (pathParams != null)
                    {
                        foreach (var pp in pathParams)
                        {
                            interpolatedPath = interpolatedPath.Replace($"{{{pp.Name}}}", $"{{System.Uri.EscapeDataString(System.Convert.ToString({pp.Name}) ?? string.Empty)}}");
                        }
                    }

                    var queryParams = operation.Parameters?.Where(p => p.In == "query").ToList();
                    if (queryParams != null && queryParams.Any())
                    {
                        var qb = new List<string>();
                        foreach (var qp in queryParams)
                        {
                            var qpType = "string";
                            if (qp.Schema != null)
                            {
                                if (qp.Schema.Type == "array")
                                {
                                    qpType = "array";
                                }
                            }
                            if (qpType == "array")
                            {
                                qb.Add($"{qp.Name}={{string.Join(\"&{qp.Name}=\", {qp.Name} != null ? System.Linq.Enumerable.Select({qp.Name}, x => System.Uri.EscapeDataString(System.Convert.ToString(x) ?? string.Empty)) : System.Linq.Enumerable.Empty<string>())}}");
                            }
                            else
                            {
                                qb.Add($"{qp.Name}={{System.Uri.EscapeDataString(System.Convert.ToString({qp.Name}) ?? string.Empty)}}");
                            }
                        }
                        interpolatedPath += "?" + string.Join("&", qb);
                    }

                    ExpressionSyntax routePathExpr;

                    if (interpolatedPath.Contains("{"))
                    {
                        routePathExpr = SyntaxFactory.ParseExpression($"$\"{interpolatedPath}\"");
                    }
                    else
                    {
                        routePathExpr = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(interpolatedPath));
                    }

                    var stmts = new List<StatementSyntax>();

                    stmts.Add(SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                        .AddVariables(
                            SyntaxFactory.VariableDeclarator("request")
                            .WithInitializer(SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Net.Http.HttpRequestMessage"))
                                .AddArgumentListArguments(
                                    SyntaxFactory.Argument(SyntaxFactory.ParseExpression($"new System.Net.Http.HttpMethod(\"{httpMethod.ToUpperInvariant()}\")")),
                                    SyntaxFactory.Argument(routePathExpr)
                                )
                            ))
                        )
                    ));

                    var headerParams = operation.Parameters?.Where(p => p.In == "header").ToList();
                    if (headerParams != null)
                    {
                        foreach (var hp in headerParams)
                        {
                            stmts.Add(SyntaxFactory.ParseStatement($"if ({hp.Name} != null) request.Headers.Add(\"{hp.Name}\", {hp.Name}.ToString());\n"));
                        }
                    }

                    if (operation.RequestBody?.Content != null && operation.RequestBody.Content.ContainsKey("application/json"))
                    {
                        stmts.Add(SyntaxFactory.ParseStatement("request.Content = System.Net.Http.Json.JsonContent.Create(body);\n"));
                    }

                    var sendCall = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("_httpClient"),
                            SyntaxFactory.IdentifierName("SendAsync")
                        )
                    ).AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request")));

                    stmts.Add(SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                        .AddVariables(
                            SyntaxFactory.VariableDeclarator("response")
                            .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.AwaitExpression(sendCall)))
                        )
                    ));

                    stmts.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("response"),
                                SyntaxFactory.IdentifierName("EnsureSuccessStatusCode")
                            )
                        )
                    ));

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

                    StatementSyntax statement3;
                    if (returnTypeName == "string")
                    {
                        statement3 = SyntaxFactory.ReturnStatement(
                            SyntaxFactory.AwaitExpression(readAsStringCall)
                        );
                    }
                    else
                    {
                        var jsonDeserializeCall = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("System.Text.Json.JsonSerializer"),
                                SyntaxFactory.GenericName("Deserialize")
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.ParseTypeName(returnTypeName)
                                        )
                                    )
                                )
                            )
                        ).AddArgumentListArguments(
                            SyntaxFactory.Argument(SyntaxFactory.AwaitExpression(readAsStringCall)),
                            SyntaxFactory.Argument(
                                SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Text.Json.JsonSerializerOptions"))
                                .WithInitializer(
                                    SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                                    .AddExpressions(
                                        SyntaxFactory.AssignmentExpression(
                                            SyntaxKind.SimpleAssignmentExpression,
                                            SyntaxFactory.IdentifierName("PropertyNameCaseInsensitive"),
                                            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)
                                        )
                                    )
                                )
                            )
                        );
                        statement3 = SyntaxFactory.ReturnStatement(jsonDeserializeCall);
                    }

                    stmts.Add(statement3);
                    methodNode = methodNode.WithBody(SyntaxFactory.Block(stmts));

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
                            if (respKvp.Value.Headers != null)
                                if (respKvp.Value.Headers.Count > 0)
                                {
                                    var headerKvp = respKvp.Value.Headers.First();
                                    attrs["header"] = headerKvp.Key;
                                    var h = headerKvp.Value;
                                    if (!string.IsNullOrEmpty(h.Description)) attrs["header-description"] = h.Description;
                                    if (h.Required.HasValue) attrs["header-required"] = h.Required.Value.ToString().ToLower();
                                    if (h.Deprecated.HasValue) attrs["header-deprecated"] = h.Deprecated.Value.ToString().ToLower();
                                    if (h.Example != null) attrs["header-example"] = h.Example.ToString()!;
                                    if (h.Examples != null)
                                        if (h.Examples.Count > 0)
                                        {
                                            attrs["header-examples"] = string.Join(",", h.Examples.Select(e => $"{e.Key}:{e.Value.Value}"));
                                        }
                                    if (!string.IsNullOrEmpty(h.Style)) attrs["header-style"] = h.Style;
                                    if (h.Explode.HasValue) attrs["header-explode"] = h.Explode.Value.ToString().ToLower();
                                    if (h.Schema?.Type != null) attrs["header-schema"] = h.Schema.Type;
                                    if (h.Content != null)
                                    {
                                        if (h.Content.Count > 0)
                                        {
                                            var c = h.Content.First();
                                            string? schemaType = null;
                                            if (c.Value.Schema != null)
                                            {
                                                schemaType = c.Value.Schema.Type;
                                            }
                                            attrs["header-content"] = $"{c.Key}:{schemaType}";
                                        }
                                    }
                                }
                            string respDesc = "Response";
                            if (respKvp.Value.Description != null)
                            {
                                respDesc = respKvp.Value.Description;
                            }
                            methodNode = Docstrings.Emit.WithTag(methodNode, "response", attrs, respDesc);
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

                    members.Add(methodNode);
                }
            }


            var mcpClassDecl = SyntaxFactory.ClassDeclaration("NativeMcpAdapter")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName(className))
                        .AddVariables(SyntaxFactory.VariableDeclarator("_client")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),
                    SyntaxFactory.ConstructorDeclaration("NativeMcpAdapter")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("client")).WithType(SyntaxFactory.ParseTypeName(className)))
                        .WithBody(SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName("_client"), SyntaxFactory.IdentifierName("client"))))),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Collections.Generic.List<object>"), "GetTools")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Collections.Generic.List<object>")).WithArgumentList(SyntaxFactory.ArgumentList())))),
                                        SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task<string>"), "ExecuteToolAsync")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("toolName")).WithType(SyntaxFactory.ParseTypeName("string")),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")).WithType(SyntaxFactory.ParseTypeName("System.Collections.Generic.Dictionary<string, object>"))
                        )
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task"), SyntaxFactory.IdentifierName("FromResult"))).AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("")))))
                        )),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Collections.Generic.List<object>"), "GetResources")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Collections.Generic.List<object>")).WithArgumentList(SyntaxFactory.ArgumentList()))))
                );

            var mcpPropDecl = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("NativeMcpAdapter"), "Mcp")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("NativeMcpAdapter")).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.ThisExpression()))))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            members.Add(mcpClassDecl);
            members.Add(mcpPropDecl);
            return classNode.AddMembers(members.ToArray());
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
