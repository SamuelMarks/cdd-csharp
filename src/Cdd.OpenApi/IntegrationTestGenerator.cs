// <copyright file="IntegrationTestGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    /// <summary>
    /// Generates integration tests.
    /// </summary>
    public static class IntegrationTestGenerator
    {
        /// <summary>
        /// Generates test code from an OpenAPI document.
        /// </summary>
        /// <param name="doc">The OpenAPI document to generate integration tests from.</param>
        /// <returns>The formatted C# integration test source code.</returns>
        public static string Generate(OpenApiDocument doc)
        {
            string basePath = "/v2/";
            if (doc?.Servers != null && doc.Servers.Count > 0 && !string.IsNullOrEmpty(doc.Servers[0].Url))
            {
                var url = doc.Servers[0].Url;
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    basePath = uri.AbsolutePath;
                }
                else
                {
                    basePath = url;
                }
            }
            else if (doc != null && !string.IsNullOrEmpty(doc.BasePath))
            {
                basePath = doc.BasePath;
            }

            if (!basePath.StartsWith("/")) basePath = "/" + basePath;
            if (!basePath.EndsWith("/")) basePath += "/";

            var fullUrl = "http://localhost:8080" + basePath;

            var members = new List<MemberDeclarationSyntax>
            {
                SyntaxFactory.ParseMemberDeclaration("private readonly HttpClient _httpClient;")!,
                SyntaxFactory.ConstructorDeclaration("IntegrationTests")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement($@"_httpClient = new HttpClient {{ BaseAddress = new Uri(""{fullUrl}"") }};"),
                        SyntaxFactory.ParseStatement(@"_httpClient.DefaultRequestHeaders.Add(""api_key"", ""special-key"");"),
                        SyntaxFactory.ParseStatement(@"_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(""Bearer"", ""special-key"");")
                    ))
            };

            var factAttr = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Fact"))));

            if (doc?.Paths != null)
            {
                foreach (var pathKvp in doc.Paths)
                {
                    var route = pathKvp.Key;
                    var pathItem = pathKvp.Value;

                    var operations = Cdd.OpenApi.Common.OperationHelpers.GetOperationsByPascalVerb(pathItem);

                    foreach (var opKvp in operations)
                    {
                        var httpMethod = opKvp.Key;
                        var operation = opKvp.Value;
                        if (operation == null) continue;

                        var methodName = operation.OperationId ?? $"{httpMethod}{route.Replace("/", "").Replace("{", "").Replace("}", "")}Async";
                        if (!methodName.EndsWith("Async")) methodName += "Async";

                        var testMethodName = "Test" + methodName;

                        var tag = "Default";
                        if (operation.Tags != null && operation.Tags.Count > 0)
                        {
                            tag = operation.Tags[0];
                        }
                        if (tag.Length > 0)
                        {
                            tag = char.ToUpperInvariant(tag[0]) + tag.Substring(1);
                        }
                        var clientName = $"{tag}ApiClient";

                        var args = new List<string>();
                        if (operation.Parameters != null)
                        {
                            foreach (var p in operation.Parameters)
                            {
                                string dummyValue = @"""string""";
                                if (p.Schema?.Type == "integer") dummyValue = "1";
                                else if (p.Schema?.Type == "number") dummyValue = "123.45";
                                else if (p.Schema?.Type == "boolean") dummyValue = "true";
                                else if (p.Schema?.Type == "array")
                                {
                                    if (p.Schema.Items != null)
                                    {
                                        if (p.Schema.Items.Ref != null)
                                        {
                                            var refName = p.Schema.Items.Ref.Split('/').Last();
                                            dummyValue = $"new List<{refName}>()";
                                        }
                                        else if (p.Schema.Items.Type != null)
                                        {
                                            var itemType = p.Schema.Items.Type == "integer" ? "int" : (p.Schema.Items.Type == "number" ? "double" : (p.Schema.Items.Type == "boolean" ? "bool" : "string"));
                                            dummyValue = $"new List<{itemType}>()";
                                        }
                                    }
                                }
                                else if (p.Schema?.Ref != null)
                                {
                                    var refName = p.Schema.Ref.Split('/').Last();
                                    dummyValue = $"new {refName}()";
                                }
                                if (p.Name == "api_key") dummyValue = @"""special-key""";
                                args.Add(dummyValue);
                            }
                        }

                        if (operation.RequestBody?.Content != null && operation.RequestBody.Content.ContainsKey("application/json"))
                        {
                            var schema = operation.RequestBody.Content["application/json"].Schema;
                            string dummyValue = @"""string""";
                            if (schema != null)
                            {
                                if (!string.IsNullOrEmpty(schema.Ref))
                                {
                                    var refName = schema.Ref.Split('/').Last();
                                    dummyValue = $"new {refName}()";
                                }
                                else if (schema.Type == "integer") dummyValue = "1";
                                else if (schema.Type == "number") dummyValue = "123.45";
                                else if (schema.Type == "boolean") dummyValue = "true";
                                else if (schema.Type == "array")
                                {
                                    if (schema.Items != null)
                                    {
                                        if (schema.Items.Ref != null)
                                        {
                                            var refName = schema.Items.Ref.Split('/').Last();
                                            dummyValue = $"new List<{refName}>()";
                                        }
                                        else if (schema.Items.Type != null)
                                        {
                                            var itemType = schema.Items.Type == "integer" ? "int" : (schema.Items.Type == "number" ? "double" : (schema.Items.Type == "boolean" ? "bool" : "string"));
                                            dummyValue = $"new List<{itemType}>()";
                                        }
                                    }
                                }
                            }
                            args.Add(dummyValue);
                        }

                        var argsString = string.Join(", ", args);

                        var tryStmt = SyntaxFactory.ParseStatement(
                            $"try {{ var client = new {clientName}(_httpClient); var response = await client.{methodName}({argsString}); Assert.NotNull(response); }} catch (HttpRequestException ex) {{ if (ex.StatusCode == System.Net.HttpStatusCode.InternalServerError) {{ throw; }} Assert.NotEqual(System.Net.HttpStatusCode.InternalServerError, ex.StatusCode); }}");

                        var testMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), testMethodName)
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                            .AddAttributeLists(factAttr)
                            .WithBody(SyntaxFactory.Block(tryStmt));

                        members.Add(testMethod);
                    }
                }
            }

            var testClass = SyntaxFactory.ClassDeclaration("IntegrationTests")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(members.ToArray());

            var ns = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("GeneratedProject.Tests"))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Xunit")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Generated.Client")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Generated.Models")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net.Http")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
                )
                .AddMembers(testClass);

            return WasmSafeRoslyn.FormatSafe(ns);
        }
    }
}
