// <copyright file="ServerRoutesGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi.Orm
{
    /// <summary>Generates the minimal API routes for the server.</summary>
    public static class ServerRoutesGenerator
    {
        /// <summary>Generates modular route extensions for the grouped OpenAPI paths.</summary>
        /// <param name="groupedPaths">The OpenAPI paths grouped by tag.</param>
        /// <param name="baseNamespace">The base C# namespace for the generated classes.</param>
        /// <param name="schemas">The OpenAPI schemas used to match route payloads and parameters to DAOs.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateRoutes(IDictionary<string, OpenApiPaths> groupedPaths, string baseNamespace, IDictionary<string, OpenApiSchema> schemas)
        {
            var results = new List<GeneratedCode>();

            foreach (var group in groupedPaths)
            {
                var tag = group.Key;
                var subPaths = group.Value;
                var statements = new List<StatementSyntax>();

                foreach (var pathKvp in subPaths)
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
                        { "Patch", pathItem.Patch }
                    };

                    foreach (var opKvp in operations)
                    {
                        var httpMethod = opKvp.Key;
                        var operation = opKvp.Value;
                        if (operation == null) continue;

                        var delegateParams = new List<string>();
                        var methodParams = new List<string>();
                        string modelType = "object";

                        if (operation.Responses != null)
                        {
                            var successResponse = Enumerable.FirstOrDefault(operation.Responses, r => r.Key.StartsWith("2"));
                            if (successResponse.Value?.Content != null && successResponse.Value.Content.TryGetValue("application/json", out var respMediaType))
                            {
                                if (respMediaType.Schema != null)
                                {
                                    if (!string.IsNullOrEmpty(respMediaType.Schema.Ref))
                                        modelType = respMediaType.Schema.Ref.Split('/').Last();
                                    else if (respMediaType.Schema.Type == "array" && respMediaType.Schema.Items != null && respMediaType.Schema.Items.Ref != null)
                                        modelType = respMediaType.Schema.Items.Ref.Split('/').Last();
                                }
                            }
                        }

                        bool hasPathId = false;
                        string pathIdType = "string";

                        if (operation.Parameters != null)
                        {
                            foreach (var p in operation.Parameters)
                            {
                                if (p.In == "path")
                                {
                                    pathIdType = p.Schema?.Type == "integer" ? "int" : "string";
                                    delegateParams.Add($"[FromRoute] {pathIdType} {p.Name}");
                                    methodParams.Add(p.Name);
                                    hasPathId = true;
                                }
                                else if (p.In == "query")
                                {
                                    string pType = p.Schema?.Type == "integer" ? "int?" : "string?";
                                    delegateParams.Add($"[FromQuery] {pType} {p.Name}");
                                }
                            }
                        }

                        string bodyType = "object";
                        bool hasBody = false;
                        if (operation.RequestBody != null)
                        {
                            hasBody = true;
                            if (operation.RequestBody.Content.TryGetValue("application/json", out var mediaType))
                            {
                                if (mediaType.Schema != null)
                                {
                                    if (!string.IsNullOrEmpty(mediaType.Schema.Ref))
                                        bodyType = mediaType.Schema.Ref.Split('/').Last();
                                    else if (mediaType.Schema.Type == "array" && mediaType.Schema.Items != null && mediaType.Schema.Items.Ref != null)
                                        bodyType = $"System.Collections.Generic.List<{mediaType.Schema.Items.Ref.Split('/').Last()}>";
                                }
                            }
                            delegateParams.Add($"[FromBody] {bodyType} body");
                        }

                        string daoType = $"I{tag}Dao";
                        string targetSchemaName = tag;

                        if (hasBody && bodyType != "object" && bodyType != "string" && !bodyType.StartsWith("System.Collections"))
                        {
                            targetSchemaName = bodyType;
                            daoType = $"I{bodyType}Dao";
                        }
                        else if (modelType != "object" && modelType != "string")
                        {
                            targetSchemaName = modelType;
                            daoType = $"I{modelType}Dao";
                        }

                        string handlerBody = "return Results.StatusCode(501);";

                        if (schemas.TryGetValue(targetSchemaName, out var initialSchema))
                        {
                            if (initialSchema.Type == "array" && initialSchema.Items?.Ref != null)
                            {
                                targetSchemaName = initialSchema.Items.Ref.Split('/').Last();
                                daoType = $"I{targetSchemaName}Dao";
                            }
                        }

                        if (schemas.TryGetValue(targetSchemaName, out var schema) && (schema.Type == "object" || schema.Properties != null))
                        {
                            delegateParams.Insert(0, $"[FromServices] {daoType} dao");

                            string expectedIdType = "string";
                            if (schema.Properties != null && schema.Properties.TryGetValue("id", out var idProp))
                            {
                                expectedIdType = idProp.Type == "integer" ? "int" : "string";
                            }

                            bool idTypeMatches = !hasPathId || (pathIdType == expectedIdType);
                            bool bodyTypeMatches = !hasBody || (bodyType == targetSchemaName);

                            handlerBody = "if (dao == null) return Results.StatusCode(501);";

                            if (httpMethod == "Get" && hasPathId && idTypeMatches)
                                handlerBody += $" return Results.Ok(await dao.GetByIdAsync({methodParams[0]}));";
                            else if (httpMethod == "Get" && !hasPathId)
                                handlerBody += " return Results.Ok(await dao.GetAllAsync());";
                            else if (httpMethod == "Post" && hasBody && bodyTypeMatches)
                                handlerBody += @" return Results.Created("""", await dao.CreateAsync(body));";
                            else if (httpMethod == "Put" && hasBody && bodyTypeMatches)
                                handlerBody += " return Results.Ok(await dao.UpdateAsync(body));";
                            else if (httpMethod == "Delete" && hasPathId && idTypeMatches)
                                handlerBody += $" await dao.DeleteAsync({methodParams[0]}); return Results.NoContent();";
                            else
                                handlerBody += " return Results.StatusCode(501);";
                        }

                        string delegateSignature = string.Join(", ", delegateParams);

                        var mapStmt = SyntaxFactory.ParseStatement(
                            $@"app.Map{httpMethod}(""{routePath}"", async ({delegateSignature}) => {{ try {{ {handlerBody} }} catch (NotImplementedException) {{ return Results.StatusCode(501); }} catch (Exception ex) {{ return Results.Problem(detail: ex.Message, statusCode: 500); }} }});");
                        statements.Add(mapStmt);
                    }
                }

                var mapMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), $"Map{tag}Routes")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("app"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.ThisKeyword))
                            .WithType(SyntaxFactory.ParseTypeName("IEndpointRouteBuilder")))
                    .WithBody(SyntaxFactory.Block(statements));

                var routesClass = SyntaxFactory.ClassDeclaration($"{tag}Routes")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .AddMembers(mapMethod);

                var routeNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Routes"))
                    .AddUsings(
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Builder")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Http")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Routing")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.Extensions.DependencyInjection")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Daos"))
                    )
                    .AddMembers(routesClass);

                results.Add(new GeneratedCode
                {
                    FileName = $"src/Routes/{tag}Routes.cs",
                    Code = WasmSafeRoslyn.FormatSafe(routeNs)
                });
            }

            return results;
        }
    }
}
