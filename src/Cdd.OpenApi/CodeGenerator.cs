using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cdd.OpenApi.Models;

namespace Cdd.OpenApi
{
    /// <summary>Auto-generated documentation for GenerateType.</summary>
    public enum GenerateType
    {
        /// <summary>Auto-generated documentation for Sdk.</summary>
        Sdk,
        /// <summary>Auto-generated documentation for SdkCli.</summary>
        SdkCli,
        /// <summary>Auto-generated documentation for Server.</summary>
        Server,
        /// <summary>Auto-generated documentation for All.</summary>
        All
    }

    /// <summary>Auto-generated documentation for GeneratedCode.</summary>
    public class GeneratedCode
    {
        /// <summary>Auto-generated documentation for FileName.</summary>
        public string FileName { get; set; } = string.Empty;
        /// <summary>Auto-generated documentation for Code.</summary>
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>Auto-generated documentation for CodeGenerator.</summary>
    public static class CodeGenerator
    {
        private static TNode AddDocTags<TNode>(TNode node, OpenApiDocument doc) where TNode : SyntaxNode
        {


            if (!string.IsNullOrEmpty(doc.Self))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "self", doc.Self);
            }
            if (!string.IsNullOrEmpty(doc.JsonSchemaDialect))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "jsonSchemaDialect", doc.JsonSchemaDialect);
            }
            if (doc.Info != null)
            {
                if (!string.IsNullOrEmpty(doc.Info.TermsOfService))
                {
                    node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "termsOfService", doc.Info.TermsOfService);
                }
            }
            if (doc.Info != null)
            {
                if (doc.Info.License != null)
                {
                    if (!string.IsNullOrEmpty(doc.Info.License.Identifier))
                    {
                        node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "license-identifier", doc.Info.License.Identifier);
                    }
                }
            }

            return node;
        }

        private static Dictionary<string, OpenApiPaths> GroupPathsByTag(OpenApiPaths paths)
        {
            var result = new Dictionary<string, OpenApiPaths>();
            foreach (var pathKvp in paths)
            {
                var routePath = pathKvp.Key;
                var pathItem = pathKvp.Value;

                var operations = new[] { pathItem.Get, pathItem.Put, pathItem.Post, pathItem.Delete, pathItem.Options, pathItem.Head, pathItem.Patch, pathItem.Trace, pathItem.Query };
                if (pathItem.AdditionalOperations != null)
                {
                    var opsList = new List<OpenApiOperation?>(operations);
                    opsList.AddRange(pathItem.AdditionalOperations.Values);
                    operations = opsList.ToArray();
                }

                bool hasOperations = false;
                foreach (var op in operations)
                {
                    if (op == null) continue;
                    hasOperations = true;
                    var tag = "Default";
                    if (op.Tags != null && op.Tags.Count > 0)
                    {
                        tag = op.Tags[0];
                    }

                    if (tag.Length > 0)
                    {
                        tag = char.ToUpperInvariant(tag[0]) + tag.Substring(1);
                    }

                    if (!result.ContainsKey(tag))
                    {
                        result[tag] = new OpenApiPaths();
                    }

                    if (!result[tag].ContainsKey(routePath))
                    {
                        var newPathItem = new OpenApiPathItem
                        {
                            Summary = pathItem.Summary,
                            Description = pathItem.Description,
                            Parameters = pathItem.Parameters,
                            Servers = pathItem.Servers,
                            Ref = pathItem.Ref
                        };
                        result[tag][routePath] = newPathItem;
                    }

                    var currentItem = result[tag][routePath];
                    if (op == pathItem.Get) currentItem.Get = op;
                    else if (op == pathItem.Put) currentItem.Put = op;
                    else if (op == pathItem.Post) currentItem.Post = op;
                    else if (op == pathItem.Delete) currentItem.Delete = op;
                    else if (op == pathItem.Options) currentItem.Options = op;
                    else if (op == pathItem.Head) currentItem.Head = op;
                    else if (op == pathItem.Patch) currentItem.Patch = op;
                    else if (op == pathItem.Trace) currentItem.Trace = op;
                    else if (op == pathItem.Query) currentItem.Query = op;
                    else if (pathItem.AdditionalOperations != null)
                    {
                        foreach (var kvp in pathItem.AdditionalOperations)
                        {
                            if (kvp.Value == op)
                            {
                                if (currentItem.AdditionalOperations == null)
                                {
                                    currentItem.AdditionalOperations = new Dictionary<string, OpenApiOperation>();
                                }
                                currentItem.AdditionalOperations[kvp.Key] = op;
                            }
                        }
                    }
                }

                if (!hasOperations)
                {
                    if (!result.ContainsKey("Default"))
                    {
                        result["Default"] = new OpenApiPaths();
                    }
                    result["Default"][routePath] = pathItem;
                }
            }
            return result;
        }

        /// <summary>Auto-generated documentation for Generate.</summary>
        public static List<GeneratedCode> Generate(OpenApiDocument doc, string baseNamespace = "Generated", GenerateType type = GenerateType.All, bool tests = false)
        {
            var results = new List<GeneratedCode>();
            if (doc == null) return results;

            if (doc.Components?.Schemas != null)
            {
                var modelsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")));

                foreach (var schemaKvp in doc.Components.Schemas)
                {
                    var classNode = Cdd.OpenApi.Classes.Emit.ToClass(schemaKvp.Key, schemaKvp.Value);
                    var singleModelNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")))
                        .AddMembers(classNode);
                    results.Add(new GeneratedCode
                    {
                        FileName = $"src/Models/{schemaKvp.Key}.cs",
                        Code = WasmSafeRoslyn.FormatSafe(singleModelNs)
                    });
                }

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    var dbContextNode = Cdd.OpenApi.Orm.Emit.ToDbContext("AppDbContext", doc.Components.Schemas);
                    var dbContextNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")))
                        .AddMembers(dbContextNode);
                    results.Add(new GeneratedCode { FileName = "src/Data/AppDbContext.cs", Code = WasmSafeRoslyn.FormatSafe(dbContextNsNode) });

                    var daoResults = Cdd.OpenApi.Orm.DaoGenerator.GenerateDaos(doc.Components.Schemas, baseNamespace);
                    results.AddRange(daoResults);

                    if (tests)
                    {
                        var daoTestResults = Cdd.OpenApi.Orm.DaoTestsGenerator.GenerateDaoTests(doc.Components.Schemas, baseNamespace);
                        results.AddRange(daoTestResults);
                    }

                    var configResults = Cdd.OpenApi.Orm.ConfigGenerator.GenerateConfig(baseNamespace);
                    results.AddRange(configResults);

                    var seederResults = Cdd.OpenApi.Orm.SeederGenerator.GenerateSeeder(doc.Components.Schemas, baseNamespace, tests);
                    results.AddRange(seederResults);

                    var routeTags = doc.Paths != null ? GroupPathsByTag(doc.Paths) : new Dictionary<string, OpenApiPaths>();

                    var entrypointResults = Cdd.OpenApi.Orm.ServerEntrypointGenerator.GenerateEntrypoint(baseNamespace, tests, routeTags.Keys);
                    results.AddRange(entrypointResults);

                    if (doc.Paths != null && doc.Paths.Count > 0)
                    {
                        var routeResults = Cdd.OpenApi.Orm.ServerRoutesGenerator.GenerateRoutes(routeTags, baseNamespace, doc.Components.Schemas);
                        results.AddRange(routeResults);
                    }

                    if (tests)
                    {
                        var configTestResults = Cdd.OpenApi.Orm.ConfigTestsGenerator.GenerateConfigTests(baseNamespace);
                        results.AddRange(configTestResults);
                    }
                }
            }

            if (doc.Paths != null && doc.Paths.Count > 0)
            {
                var attributesCode = @"namespace " + baseNamespace + @"
{
    using System;
    /// <summary>Auto-generated documentation for ExplodeAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ExplodeAttribute : Attribute {}
    /// <summary>Auto-generated documentation for AllowEmptyValueAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AllowEmptyValueAttribute : Attribute {}
    /// <summary>Auto-generated documentation for AllowReservedAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class AllowReservedAttribute : Attribute {}
    /// <summary>Auto-generated documentation for StyleAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class StyleAttribute : Attribute {
        /// <summary>Auto-generated documentation for StyleAttribute.</summary>
        public StyleAttribute(string style) {}
    }
    /// <summary>Auto-generated documentation for ContentAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ContentAttribute : Attribute {
        /// <summary>Auto-generated documentation for ContentAttribute.</summary>
        public ContentAttribute(string mediaType, string schemaType) {}
    }
    /// <summary>Auto-generated documentation for ExamplesAttribute.</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class ExamplesAttribute : Attribute {
        /// <summary>Auto-generated documentation for ExamplesAttribute.</summary>
        public ExamplesAttribute(params string[] args) {}
    }
}
";
                results.Add(new GeneratedCode { FileName = "Attributes.cs", Code = attributesCode });

                var groupedPaths = GroupPathsByTag(doc.Paths);

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    foreach (var group in groupedPaths)
                    {
                        var tag = group.Key;
                        var subPaths = group.Value;
                        var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface($"I{tag}Api", subPaths);
                        interfaceNode = AddDocTags(interfaceNode, doc);
                        var nsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Api"))
                            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")))
                            .AddMembers(interfaceNode);
                        results.Add(new GeneratedCode { FileName = $"src/Api/I{tag}Api.cs", Code = WasmSafeRoslyn.FormatSafe(nsNode) });
                    }
                }

                if (type == GenerateType.All || type == GenerateType.Sdk)
                {
                    foreach (var group in groupedPaths)
                    {
                        var tag = group.Key;
                        var subPaths = group.Value;
                        var clientNode = Cdd.OpenApi.Clients.Emit.ToClient($"{tag}ApiClient", subPaths);
                        clientNode = AddDocTags(clientNode, doc);
                        var clientNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Client"))
                            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                                       SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net.Http.Json")))
                            .AddMembers(clientNode);
                        results.Add(new GeneratedCode { FileName = $"src/Client/{tag}ApiClient.cs", Code = WasmSafeRoslyn.FormatSafe(clientNsNode) });

                        if (tests)
                        {
                            var clientTestsNode = Cdd.OpenApi.TestsModule.Emit.ToClientTests($"{tag}ApiClientTests", subPaths, tests);
                            clientTestsNode = AddDocTags(clientTestsNode, doc);
                            var clientTestsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Client")))
                                .AddMembers(clientTestsNode);
                            results.Add(new GeneratedCode { FileName = $"src/Tests/{tag}ApiClientTests.cs", Code = WasmSafeRoslyn.FormatSafe(clientTestsNsNode) });
                        }
                    }
                }

                if (type == GenerateType.All || type == GenerateType.SdkCli)
                {
                    foreach (var group in groupedPaths)
                    {
                        var tag = group.Key;
                        var subPaths = group.Value;
                        var cliNode = Cdd.OpenApi.CliModule.Emit.ToCli($"{tag}ApiClientCli", subPaths);
                        cliNode = AddDocTags(cliNode, doc);
                        var cliNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Cli"))
                            .AddMembers(cliNode);
                        results.Add(new GeneratedCode { FileName = $"src/Cli/{tag}ApiClientCli.cs", Code = WasmSafeRoslyn.FormatSafe(cliNsNode) });
                    }

                    var mcpServerNode = Cdd.OpenApi.Mcp.Emit.ToMcpServer("McpServer", doc.Paths);
                    var mcpServerNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Cli"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.IO")))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")))
                        .AddMembers(mcpServerNode);
                    results.Add(new GeneratedCode { FileName = "src/Cli/McpServer.cs", Code = WasmSafeRoslyn.FormatSafe(mcpServerNsNode) });

                    var mcpModelsNode = Cdd.OpenApi.Mcp.Emit.ToMcpModels();
                    var mcpModelsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Cli.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")))
                        .AddMembers(mcpModelsNode.ToArray());
                    results.Add(new GeneratedCode { FileName = "src/Cli/McpModels.cs", Code = WasmSafeRoslyn.FormatSafe(mcpModelsNsNode) });
                }

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    foreach (var group in groupedPaths)
                    {
                        var tag = group.Key;
                        var subPaths = group.Value;
                        var mockNode = Cdd.OpenApi.Mocks.Emit.ToMock($"{tag}ApiMock", subPaths, tests);
                        mockNode = AddDocTags(mockNode, doc);
                        var mockNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Mocks"))
                            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Api")))
                            .AddMembers(mockNode);
                        results.Add(new GeneratedCode { FileName = $"src/Mocks/{tag}ApiMock.cs", Code = WasmSafeRoslyn.FormatSafe(mockNsNode) });

                        var testsNode = Cdd.OpenApi.TestsModule.Emit.ToTests($"{tag}ApiTests", subPaths, tests);
                        testsNode = AddDocTags(testsNode, doc);
                        var testsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Api")))
                            .AddMembers(testsNode);
                        results.Add(new GeneratedCode { FileName = $"src/Tests/{tag}ApiTests.cs", Code = WasmSafeRoslyn.FormatSafe(testsNsNode) });
                    }
                }
            }

            return results;
        }
    }
}
