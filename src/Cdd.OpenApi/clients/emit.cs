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

namespace Cdd.OpenApi.Clients
{
    /// <summary>Emits strongly-typed C# HTTP client classes from <see cref="Cdd.OpenApi.Ir.IrEndpoint"/> and <see cref="OpenApiPaths"/> definitions.</summary>
    public static class Emit
    {
        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> representing an asynchronous HTTP API client from IR endpoints.
        /// </summary>
        /// <param name="className">The name of the generated client class.</param>
        /// <param name="endpoints">The IR endpoints to generate client methods for.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
        public static ClassDeclarationSyntax ToClient(string className, IReadOnlyList<Cdd.OpenApi.Ir.IrEndpoint> endpoints)
        {
            return ToClient(className, OperationHelpers.ConvertEndpointsToPaths(endpoints));
        }

        /// <summary>
        /// Generates a Roslyn <see cref="ClassDeclarationSyntax"/> representing an asynchronous HTTP API client.
        /// </summary>
        /// <param name="className">The name of the generated client class.</param>
        /// <param name="paths">The OpenAPI paths to generate client methods for.</param>
        /// <returns>The generated <see cref="ClassDeclarationSyntax"/>.</returns>
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
                        SyntaxFactory.IdentifierName("httpClient"))));

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

                var operations = OperationHelpers.GetOperationsByPascalVerb(pathItem);

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
                        if (successResponse.Value?.Content != null && successResponse.Value.Content.TryGetValue("application/json", out var mediaType))
                        {
                            var schema = mediaType.Schema;
                            if (schema != null)
                            {
                                returnTypeName = ResolveClientSchemaType(schema);
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
                            string paramType = p.Schema != null ? ResolveClientSchemaType(p.Schema) : "string";
                            var pNode = SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(paramType));

                            pNode = OperationSignatureBuilder.ApplyParameterMetadata(pNode, p);
                            parameters.Add(pNode);
                        }
                    }

                    if (operation.RequestBody?.Content != null &&
                        operation.RequestBody.Content.TryGetValue("application/json", out var reqMediaType) &&
                        reqMediaType.Schema != null)
                    {
                        string typeName = ResolveClientSchemaType(reqMediaType.Schema);
                        parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("body"))
                            .WithType(SyntaxFactory.ParseTypeName(typeName)));
                    }

                    if (parameters.Any())
                    {
                        methodNode = methodNode.AddParameterListParameters(parameters.ToArray());
                    }

                    string interpolatedPath = routePath.TrimStart('/');

                    if (operation.Parameters != null)
                    {
                        var pathParams = operation.Parameters.Where(p => p.In == "path").ToList();
                        foreach (var pp in pathParams)
                        {
                            interpolatedPath = interpolatedPath.Replace($"{{{pp.Name}}}", $"{{System.Uri.EscapeDataString(System.Convert.ToString({pp.Name}) ?? string.Empty)}}");
                        }

                        var queryParams = operation.Parameters.Where(p => p.In == "query").ToList();
                        if (queryParams.Any())
                        {
                            var qb = new List<string>();
                            foreach (var qp in queryParams)
                            {
                                var qpType = "string";
                                if (qp.Schema != null && qp.Schema.Type == OpenApiDataType.Array)
                                {
                                    qpType = OpenApiDataType.Array;
                                }

                                if (qpType == OpenApiDataType.Array)
                                {
                                    qb.Add($$"""{{qp.Name}}={string.Join("&{{qp.Name}}=", {{qp.Name}} != null ? System.Linq.Enumerable.Select({{qp.Name}}, x => System.Uri.EscapeDataString(System.Convert.ToString(x) ?? string.Empty)) : System.Linq.Enumerable.Empty<string>())}""");
                                }
                                else
                                {
                                    qb.Add($$"""{{qp.Name}}={System.Uri.EscapeDataString(System.Convert.ToString({{qp.Name}}) ?? string.Empty)}""");
                                }
                            }
                            interpolatedPath += "?" + string.Join("&", qb);
                        }
                    }

                    ExpressionSyntax routePathExpr = interpolatedPath.Contains("{")
                        ? SyntaxFactory.ParseExpression($$"""$"{{interpolatedPath}}" """)
                        : SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(interpolatedPath));

                    var stmts = new List<StatementSyntax>
                    {
                        SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                                .AddVariables(
                                    SyntaxFactory.VariableDeclarator("request")
                                        .WithInitializer(SyntaxFactory.EqualsValueClause(
                                            SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Net.Http.HttpRequestMessage"))
                                                .AddArgumentListArguments(
                                                    SyntaxFactory.Argument(SyntaxFactory.ParseExpression($$"""new System.Net.Http.HttpMethod("{{httpMethod.ToUpperInvariant()}}")""")),
                                                    SyntaxFactory.Argument(routePathExpr))))))
                    };

                    var headerParams = operation.Parameters?.Where(p => p.In == "header").ToList();
                    if (headerParams != null)
                    {
                        foreach (var hp in headerParams)
                        {
                            stmts.Add(SyntaxFactory.ParseStatement($$"""if ({{hp.Name}} != null) request.Headers.Add("{{hp.Name}}", {{hp.Name}}.ToString());"""));
                        }
                    }

                    if (operation.RequestBody?.Content != null && operation.RequestBody.Content.ContainsKey("application/json"))
                    {
                        stmts.Add(SyntaxFactory.ParseStatement("request.Content = System.Net.Http.Json.JsonContent.Create(body);"));
                    }

                    var sendCall = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("_httpClient"),
                            SyntaxFactory.IdentifierName("SendAsync")))
                        .AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("request")));

                    stmts.Add(SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                            .AddVariables(
                                SyntaxFactory.VariableDeclarator("response")
                                    .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.AwaitExpression(sendCall))))));

                    stmts.Add(SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("response"),
                                SyntaxFactory.IdentifierName("EnsureSuccessStatusCode")))));

                    var readAsStringCall = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName("response"),
                                SyntaxFactory.IdentifierName("Content")),
                            SyntaxFactory.IdentifierName("ReadAsStringAsync")));

                    StatementSyntax statement3;
                    if (returnTypeName == "string")
                    {
                        statement3 = SyntaxFactory.ReturnStatement(SyntaxFactory.AwaitExpression(readAsStringCall));
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
                                                SyntaxFactory.ParseTypeName(returnTypeName))))))
                            .AddArgumentListArguments(
                                SyntaxFactory.Argument(SyntaxFactory.AwaitExpression(readAsStringCall)),
                                SyntaxFactory.Argument(
                                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Text.Json.JsonSerializerOptions"))
                                        .WithInitializer(
                                            SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                                                .AddExpressions(
                                                    SyntaxFactory.AssignmentExpression(
                                                        SyntaxKind.SimpleAssignmentExpression,
                                                        SyntaxFactory.IdentifierName("PropertyNameCaseInsensitive"),
                                                        SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression))))));
                        statement3 = SyntaxFactory.ReturnStatement(jsonDeserializeCall);
                    }

                    stmts.Add(statement3);
                    methodNode = methodNode.WithBody(SyntaxFactory.Block(stmts));
                    methodNode = OperationSignatureBuilder.ApplyOperationDocstrings(methodNode, operation, includeParamDocs: true);

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
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("args")).WithType(SyntaxFactory.ParseTypeName("System.Collections.Generic.Dictionary<string, object>")))
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ReturnStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseTypeName("System.Threading.Tasks.Task"), SyntaxFactory.IdentifierName("FromResult"))).AddArgumentListArguments(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(""))))))),
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("System.Collections.Generic.List<object>"), "GetResources")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("System.Collections.Generic.List<object>")).WithArgumentList(SyntaxFactory.ArgumentList())))));

            var mcpPropDecl = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("NativeMcpAdapter"), "Mcp")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("NativeMcpAdapter")).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.ThisExpression()))))))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            members.Add(mcpClassDecl);
            members.Add(mcpPropDecl);
            return classNode.AddMembers(members.ToArray());
        }

        /// <summary>
        /// Resolves a client-facing C# type name for an <see cref="OpenApiSchema"/>.
        /// </summary>
        /// <param name="schema">The OpenAPI schema to resolve.</param>
        /// <returns>The resolved C# type name.</returns>
        private static string ResolveClientSchemaType(OpenApiSchema schema)
        {
            if (!string.IsNullOrEmpty(schema.Ref))
            {
                return schema.Ref.Split('/').Last();
            }

            if (schema.Type == OpenApiDataType.Array)
            {
                if (schema.Items?.Ref != null)
                {
                    return $"System.Collections.Generic.List<{schema.Items.Ref.Split('/').Last()}>";
                }

                if (schema.Items?.Type != null)
                {
                    return $"System.Collections.Generic.List<{TypeMapper.MapOpenApiToCSharp(schema.Items.Type, "string")}>";
                }
            }
            else if (schema.Type != null)
            {
                return TypeMapper.MapOpenApiToCSharp(schema.Type, "string");
            }
            else if (schema.Items?.Ref != null)
            {
                return $"System.Collections.Generic.List<{schema.Items.Ref.Split('/').Last()}>";
            }

            return "string";
        }
    }
}
