// <copyright file="CodeGenerator.cs" company="Cdd.OpenApi">
// Copyright (c) Cdd.OpenApi. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Cdd.OpenApi.Ir;
using Cdd.OpenApi.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cdd.OpenApi
{
    /// <summary>Specifies the target C# artifacts to generate from an OpenAPI document.</summary>
    public enum GenerateType
    {
        /// <summary>Generates client SDK models and HTTP client classes.</summary>
        Sdk,

        /// <summary>Generates client SDK models, HTTP client classes, and a CLI wrapper.</summary>
        SdkCli,

        /// <summary>Generates ASP.NET Core server interfaces, routes, DAOs, EF Core DbContext, and seeders.</summary>
        Server,

        /// <summary>Generates all SDK, CLI, Server, ORM, and optional MCP artifacts.</summary>
        All
    }

    /// <summary>Represents a single generated C# source file and its relative file path.</summary>
    public class GeneratedCode
    {
        /// <summary>Gets or sets the relative file path of the generated C# file.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>Gets or sets the formatted C# source code content.</summary>
        public string Code { get; set; } = string.Empty;
    }

    /// <summary>Generates in-memory C# source files from an <see cref="OpenApiDocument"/> or <see cref="IrService"/>.</summary>
    public static class CodeGenerator
    {
        /// <summary>
        /// Attaches top-level service metadata as XML documentation tags onto a syntax node.
        /// </summary>
        /// <typeparam name="TNode">The syntax node type.</typeparam>
        /// <param name="node">The syntax node to annotate.</param>
        /// <param name="ir">The source IR service.</param>
        /// <returns>The annotated syntax node.</returns>
        private static TNode AddDocTags<TNode>(TNode node, IrService ir)
            where TNode : SyntaxNode
        {
            if (!string.IsNullOrEmpty(ir.Self))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "self", ir.Self);
            }

            if (!string.IsNullOrEmpty(ir.JsonSchemaDialect))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "jsonSchemaDialect", ir.JsonSchemaDialect);
            }

            if (!string.IsNullOrEmpty(ir.TermsOfService))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "termsOfService", ir.TermsOfService);
            }

            if (ir.License != null && !string.IsNullOrEmpty(ir.License.Identifier))
            {
                node = Cdd.OpenApi.Docstrings.Emit.WithTag(node, "license-identifier", ir.License.Identifier);
            }

            return node;
        }

        /// <summary>
        /// Groups the endpoints in an <see cref="IrService"/> by their primary operation tag, merging path-level parameters non-destructively.
        /// </summary>
        /// <param name="ir">The IR service containing endpoints and path metadata.</param>
        /// <returns>A dictionary mapping normalized tag names to their corresponding <see cref="IrEndpoint"/> lists.</returns>
        private static Dictionary<string, List<IrEndpoint>> GroupEndpointsByTag(IrService ir)
        {
            var result = new Dictionary<string, List<IrEndpoint>>();
            var pathsWithEndpoints = new HashSet<string>();

            foreach (var ep in ir.Endpoints)
            {
                pathsWithEndpoints.Add(ep.Path);
                var tag = "Default";
                if (ep.Tags != null && ep.Tags.Count > 0 && !string.IsNullOrEmpty(ep.Tags[0]))
                {
                    tag = ep.Tags[0];
                    tag = char.ToUpperInvariant(tag[0]) + tag.Substring(1);
                }

                if (!result.TryGetValue(tag, out var list))
                {
                    list = new List<IrEndpoint>();
                    result[tag] = list;
                }

                var mergedParams = new List<IrParameter>();
                var existingNames = new HashSet<string>(ep.Parameters.Select(p => $"{p.Location}:{p.Name}"));
                var pathParams = ir.PathMetadata[ep.Path].Parameters;
                if (pathParams != null)
                {
                    foreach (var pathParam in pathParams)
                    {
                        if (!existingNames.Contains($"{pathParam.In}:{pathParam.Name}"))
                        {
                            mergedParams.Add(IrParameter.FromOpenApiParameter(pathParam));
                        }
                    }
                }

                mergedParams.AddRange(ep.Parameters);

                var clonedEndpoint = new IrEndpoint
                {
                    Path = ep.Path,
                    HttpMethod = ep.HttpMethod,
                    OperationId = ep.OperationId,
                    HasExplicitOperationId = ep.HasExplicitOperationId,
                    Summary = ep.Summary,
                    Description = ep.Description,
                    Tags = ep.Tags,
                    ExternalDocs = ep.ExternalDocs,
                    Deprecated = ep.Deprecated,
                    RequestBody = ep.RequestBody,
                    IrBody = ep.IrBody,
                    Callbacks = ep.Callbacks,
                    Security = ep.Security,
                    Servers = ep.Servers,
                    Parameters = mergedParams,
                    Responses = ep.Responses
                };

                list.Add(clonedEndpoint);
            }

            foreach (var metaKvp in ir.PathMetadata)
            {
                if (!pathsWithEndpoints.Contains(metaKvp.Key))
                {
                    if (!result.ContainsKey("Default"))
                    {
                        result["Default"] = new List<IrEndpoint>();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generates C# source files from an <see cref="OpenApiDocument"/>.
        /// </summary>
        /// <param name="doc">The OpenAPI document to generate code from.</param>
        /// <param name="baseNamespace">The root C# namespace for generated files.</param>
        /// <param name="type">The target generation mode (All, Sdk, SdkCli, Server).</param>
        /// <param name="tests">Whether to generate composable unit/integration test suites and mocks.</param>
        /// <param name="mcp">Whether to explicitly generate MCP server and model files regardless of target type.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> Generate(OpenApiDocument doc, string baseNamespace = "Generated", GenerateType type = GenerateType.All, bool tests = false, bool mcp = false)
        {
            if (doc == null) return new List<GeneratedCode>();
            return GenerateFromIr(IrService.FromOpenApi(doc), baseNamespace, type, tests, mcp);
        }

        /// <summary>
        /// Generates C# source files from an <see cref="IrService"/>.
        /// </summary>
        /// <param name="ir">The IR service to generate code from.</param>
        /// <param name="baseNamespace">The root C# namespace for generated files.</param>
        /// <param name="type">The target generation mode (All, Sdk, SdkCli, Server).</param>
        /// <param name="tests">Whether to generate composable unit/integration test suites and mocks.</param>
        /// <param name="mcp">Whether to explicitly generate MCP server and model files regardless of target type.</param>
        /// <returns>A list of generated source code files.</returns>
        public static List<GeneratedCode> GenerateFromIr(IrService ir, string baseNamespace = "Generated", GenerateType type = GenerateType.All, bool tests = false, bool mcp = false)
        {
            var results = new List<GeneratedCode>();

            var hasPaths = ir.Endpoints.Count > 0 || ir.PathMetadata.Count > 0;
            var groupedEndpoints = hasPaths ? GroupEndpointsByTag(ir) : new Dictionary<string, List<IrEndpoint>>();

            if (ir.Models.Count > 0 || ir.HasEmptySchemasObject)
            {
                foreach (var model in ir.Models)
                {
                    var classNode = Cdd.OpenApi.Classes.Emit.ToClass(model);
                    var singleModelNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization")))
                        .AddMembers(classNode);
                    results.Add(new GeneratedCode
                    {
                        FileName = $"src/Models/{model.Name}.cs",
                        Code = WasmSafeRoslyn.FormatSafe(singleModelNs)
                    });
                }

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    var ormModels = ir.Models
                        .Where(m => m.Fields.Count > 0 || m.Schema!.Type == "object")
                        .ToList();

                    var dbContextNode = Cdd.OpenApi.Orm.Emit.ToDbContext("AppDbContext", ormModels);
                    var dbContextNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Models"))
                        .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")))
                        .AddMembers(dbContextNode);
                    results.Add(new GeneratedCode { FileName = "src/Data/AppDbContext.cs", Code = WasmSafeRoslyn.FormatSafe(dbContextNsNode) });

                    var daoResults = Cdd.OpenApi.Orm.DaoGenerator.GenerateDaos(ormModels, baseNamespace);
                    results.AddRange(daoResults);

                    if (tests)
                    {
                        var daoTestResults = Cdd.OpenApi.Orm.DaoTestsGenerator.GenerateDaoTests(ormModels, baseNamespace);
                        results.AddRange(daoTestResults);
                    }

                    var configResults = Cdd.OpenApi.Orm.ConfigGenerator.GenerateConfig(baseNamespace);
                    results.AddRange(configResults);

                    var seederResults = Cdd.OpenApi.Orm.SeederGenerator.GenerateSeeder(ormModels, baseNamespace, tests);
                    results.AddRange(seederResults);

                    var entrypointResults = Cdd.OpenApi.Orm.ServerEntrypointGenerator.GenerateEntrypoint(baseNamespace, tests, groupedEndpoints.Keys);
                    results.AddRange(entrypointResults);

                    if (hasPaths)
                    {
                        var routeResults = Cdd.OpenApi.Orm.ServerRoutesGenerator.GenerateRoutes(groupedEndpoints, baseNamespace, ir.Models);
                        results.AddRange(routeResults);
                    }

                    if (tests)
                    {
                        var configTestResults = Cdd.OpenApi.Orm.ConfigTestsGenerator.GenerateConfigTests(baseNamespace);
                        results.AddRange(configTestResults);
                    }
                }
            }

            if (hasPaths)
            {
                var attrUsage = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("AttributeUsage"))
                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression("AttributeTargets.All")))))));
                var attrBase = SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(
                    SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("Attribute"))));

                ClassDeclarationSyntax MakeAttr(string name, params ParameterSyntax[] ctorParams)
                {
                    var cls = SyntaxFactory.ClassDeclaration(name)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(attrUsage)
                        .WithBaseList(attrBase);
                    if (ctorParams.Length > 0)
                    {
                        cls = cls.AddMembers(
                            SyntaxFactory.ConstructorDeclaration(name)
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(ctorParams)
                                .WithBody(SyntaxFactory.Block()));
                    }

                    return cls;
                }

                var attrNs = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(baseNamespace))
                    .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                    .AddMembers(
                        MakeAttr("ExplodeAttribute"),
                        MakeAttr("AllowEmptyValueAttribute"),
                        MakeAttr("AllowReservedAttribute"),
                        MakeAttr("StyleAttribute", SyntaxFactory.Parameter(SyntaxFactory.Identifier("style")).WithType(SyntaxFactory.ParseTypeName("string"))),
                        MakeAttr("ContentAttribute",
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("mediaType")).WithType(SyntaxFactory.ParseTypeName("string")),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("schemaType")).WithType(SyntaxFactory.ParseTypeName("string"))),
                        MakeAttr("ExamplesAttribute",
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("args"))
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ParamsKeyword))
                                .WithType(SyntaxFactory.ParseTypeName("string[]")))
                    );
                results.Add(new GeneratedCode { FileName = "Attributes.cs", Code = WasmSafeRoslyn.FormatSafe(attrNs) });

                if (type == GenerateType.All || type == GenerateType.Server)
                {
                    foreach (var group in groupedEndpoints)
                    {
                        var tag = group.Key;
                        var subEndpoints = group.Value;
                        var interfaceNode = Cdd.OpenApi.Routes.Emit.ToInterface($"I{tag}Api", subEndpoints, ir.PathMetadata);
                        interfaceNode = AddDocTags(interfaceNode, ir);
                        var nsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Api"))
                            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")))
                            .AddMembers(interfaceNode);
                        results.Add(new GeneratedCode { FileName = $"src/Api/I{tag}Api.cs", Code = WasmSafeRoslyn.FormatSafe(nsNode) });
                    }
                }

                if (type == GenerateType.All || type == GenerateType.Sdk)
                {
                    foreach (var group in groupedEndpoints)
                    {
                        var tag = group.Key;
                        var subEndpoints = group.Value;
                        var clientNode = Cdd.OpenApi.Clients.Emit.ToClient($"{tag}ApiClient", subEndpoints);
                        clientNode = AddDocTags(clientNode, ir);
                        var clientNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Client"))
                            .AddUsings(
                                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Models")),
                                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net.Http.Json")))
                            .AddMembers(clientNode);
                        results.Add(new GeneratedCode { FileName = $"src/Client/{tag}ApiClient.cs", Code = WasmSafeRoslyn.FormatSafe(clientNsNode) });

                        if (tests)
                        {
                            var clientTestsNode = Cdd.OpenApi.TestsModule.Emit.ToClientTests($"{tag}ApiClientTests", subEndpoints, tests);
                            clientTestsNode = AddDocTags(clientTestsNode, ir);
                            var clientTestsNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Tests"))
                                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Client")))
                                .AddMembers(clientTestsNode);
                            results.Add(new GeneratedCode { FileName = $"src/Tests/{tag}ApiClientTests.cs", Code = WasmSafeRoslyn.FormatSafe(clientTestsNsNode) });
                        }
                    }
                }

                if (type == GenerateType.All || type == GenerateType.SdkCli)
                {
                    foreach (var group in groupedEndpoints)
                    {
                        var tag = group.Key;
                        var subEndpoints = group.Value;
                        var cliNode = Cdd.OpenApi.CliModule.Emit.ToCli($"{tag}ApiClientCli", subEndpoints);
                        cliNode = AddDocTags(cliNode, ir);
                        var cliNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Cli"))
                            .AddMembers(cliNode);
                        results.Add(new GeneratedCode { FileName = $"src/Cli/{tag}ApiClientCli.cs", Code = WasmSafeRoslyn.FormatSafe(cliNsNode) });
                    }
                }

                if (type == GenerateType.All || type == GenerateType.SdkCli || mcp)
                {
                    var mcpServerNode = Cdd.OpenApi.Mcp.Emit.ToMcpServerFromIr("McpServer", ir.Endpoints);
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
                    foreach (var group in groupedEndpoints)
                    {
                        var tag = group.Key;
                        var subEndpoints = group.Value;
                        var mockNode = Cdd.OpenApi.Mocks.Emit.ToMock($"{tag}ApiMock", subEndpoints, tests);
                        mockNode = AddDocTags(mockNode, ir);
                        var mockNsNode = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"{baseNamespace}.Mocks"))
                            .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName($"{baseNamespace}.Api")))
                            .AddMembers(mockNode);
                        results.Add(new GeneratedCode { FileName = $"src/Mocks/{tag}ApiMock.cs", Code = WasmSafeRoslyn.FormatSafe(mockNsNode) });

                        var testsNode = Cdd.OpenApi.TestsModule.Emit.ToTests($"{tag}ApiTests", subEndpoints, tests);
                        testsNode = AddDocTags(testsNode, ir);
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
